using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using NullReferenceException = System.NullReferenceException;

namespace TerraUtilDevTools;

public partial class FixedModBuilding : ILoadable
{
    private static ILog Logger => DevTools.Instance.Logger;

    [GeneratedRegex(@"^\s*(\d+) Error\(s\)", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex ErrorRegex();

    [GeneratedRegex(@"^\s*(\d+) Warning\(s\)", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex WarningRegex();

    [GeneratedRegex(@"error \w*:", RegexOptions.Compiled)]
    private static partial Regex ErrorMessageRegex();

    public void Load(Mod mod)
    {
        // Make the mod building use dotnet build
        var modCompileType = typeof(Main).Assembly.GetType("Terraria.ModLoader.Core.ModCompile");
        var buildMethod = modCompileType?.GetMethod("Build", BindingFlags.Instance | BindingFlags.NonPublic, [typeof(string)]);

        if (buildMethod is null)
            throw new NullReferenceException("Unable to edit Build(string) in ModCompile");

        MonoModHooks.Add(buildMethod, OnModCompileBuild);
    }

    public void Unload() { }

    // Naughty, not always calling orig in detour!
    private static void OnModCompileBuild(Action<object, string> orig, object self, string modFolder)
    {
        // Don't worry about command line builds (for mods that don't use TerraUtil), mods aren't loaded
        BuildMod(modFolder);
    }

    private static void BuildMod(string modFolder)
    {
        Logger.Debug("Overriding tML build system, using dotnet build");

        // TODO: maybe add progress bar updating
        // Load mod info
        // TODO: status.SetStatus(Language.GetTextValue("tModLoader.ReadingProperties", modName));

        if (modFolder.EndsWith('\\') || modFolder.EndsWith('/'))
            modFolder = modFolder[..^1];

        string modName = Path.GetFileName(modFolder);
        string file = Path.Combine(ModLoader.ModPath, modName + ".tmod");

        var modFileType = typeof(TmodFile);
        var ctor = modFileType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, [typeof(string), typeof(string), typeof(Version)]);
        if (ctor is null)
            throw new NullReferenceException("Failed to get TmodFile.ctor(string, string, Version)");

        var modFile = (TmodFile)ctor.Invoke([file, modName, null]);

        // Build the mod
        try
        {
            // TODO: status.SetStatus(Language.GetTextValue("tModLoader.Building", mod.Name));

            // Unload the mod
            if (ModLoader.TryGetMod(modName, out var loadedMod))
                loadedMod.Close();

            // Run dotnet build
            string outputPath = modFile.path;
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet", // TODO: UIModSources.GetSystemDotnetPath() ?? "dotnet",
                    Arguments = $"build --no-incremental -c Release -v q -p:OutputTmodPath=\"{outputPath}\"",
                    WorkingDirectory = modFolder,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
            };

            // Force locale to be in English
            // Needed because of how we are getting the error and warning count
            process.StartInfo.EnvironmentVariables["DOTNET_CLI_UI_LANGUAGE"] = "en-US";

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit(1000 * 60); // Wait up to a minute for the process to end

            // Build succeeded!
            if (process is { HasExited: true, ExitCode: 0 })
            {
                // TODO: LocalizationLoader.HandleModBuilt(mod.Name);
                return;
            }

            // Build failed :(
            Logger.Error("Complete build output:\n" + output);
            Logger.Error("Stderr:\n" + stderr);

            var errorMatch = ErrorRegex().Match(output);
            var warningMatch = WarningRegex().Match(output);
            string numErrors = errorMatch.Success ? errorMatch.Groups[1].Value : "?";
            string numWarnings = warningMatch.Success ? warningMatch.Groups[1].Value : "?";

            string firstError = output.Split('\n').FirstOrDefault(line => ErrorMessageRegex().IsMatch(line), "N/A");

            throw new Exception(Language.GetTextValue("tModLoader.CompileError", modName + ".dll", numErrors, numWarnings) + $"\nError: {firstError}");
        }
        catch (Exception e)
        {
            e.Data["mod"] = modName;
            throw;
        }
    }
}
