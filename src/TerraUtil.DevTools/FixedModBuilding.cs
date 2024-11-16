using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerraUtilDevTools;

public class FixedModBuilding : ILoadable
{
    private static ILog Logger => DevTools.Instance.Logger;

    // Make the mod building use dotnet build
    public void Load(Mod mod)
    {
        var uiBuildModType = typeof(Main).Assembly.GetType("Terraria.ModLoader.UI.UIBuildMod");
        mod.Logger.Debug(uiBuildModType);
        var buildModMethod = uiBuildModType?.GetMethod("BuildMod", BindingFlags.Instance | BindingFlags.NonPublic);
        mod.Logger.Debug(buildModMethod);

        if (buildModMethod is null)
            throw new NullReferenceException("Unable to edit BuildMod in UIBuildMod");

        MonoModHooks.Modify(buildModMethod, ModifyBuildMod);
    }

    public void Unload() { }

    private static void ModifyBuildMod(ILContext il)
    {
        var cursor = new ILCursor(il);

        // Go to the build call
        cursor.GotoNext(MoveType.After, i => i.OpCode == OpCodes.Newobj);
        cursor.GotoNext(MoveType.After, i => i.OpCode == OpCodes.Newobj);

        var newBuildModMethod = typeof(FixedModBuilding).GetMethod(nameof(BuildMod), BindingFlags.Static | BindingFlags.NonPublic);
        if (newBuildModMethod is null)
            throw new NullReferenceException("Unable to get new BuildMod method");

        cursor.EmitCall(newBuildModMethod);

        var label = cursor.MarkLabel();
        cursor.EmitBr(label);
        cursor.Index++;
        cursor.MarkLabel(label);
    }

    // TODO: get the mod's name
    private static void BuildMod(string modFolder)
    {
        Logger.Debug("Overriding tML build system, using dotnet build");

        try
        {
            status.SetStatus(Language.GetTextValue("tModLoader.Building", mod.Name));

            string csprojFile = Path.Combine(mod.path, mod.Name);
            csprojFile = Path.ChangeExtension(csprojFile, ".csproj");
            if (!File.Exists(csprojFile))
            {
                throw new BuildException(Language.GetTextValue("tModLoader.BuildErrorMissingCsproj"));
            }

            if (ModLoader.TryGetMod(mod.Name, out var loadedMod))
            {
                loadedMod.Close();
            }

            string outputPath = mod.modFile.path;
            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = UIModSources.GetSystemDotnetPath() ?? "dotnet",
                    Arguments = $"build --no-incremental -c Release -v q -p:OutputTmodPath=\"{outputPath}\"",
                    WorkingDirectory = mod.path,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
            };

            // Force locale to be in English.
            // Needed because of how we are getting the error and warning count.
            process.StartInfo.EnvironmentVariables["DOTNET_CLI_UI_LANGUAGE"] = "en-US";

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit(1000 * 60); // Wait up to a minute for the process to end

            if (!process.HasExited || process.ExitCode != 0)
            {
                Logger.Debug("Complete build output:\n" + output);
                Logger.Debug("Stderr:\n" + stderr);

                var errorMatch = ErrorRegex.Match(output);
                var warningMatch = WarningRegex.Match(output);
                string numErrors = errorMatch.Success ? errorMatch.Groups[1].Value : "?";
                string numWarnings = warningMatch.Success ? warningMatch.Groups[1].Value : "?";

                string firstError = output.Split('\n').FirstOrDefault(line => ErrorMessageRegex.IsMatch(line), "N/A");

                throw new BuildException(Language.GetTextValue("tModLoader.CompileError", mod.Name + ".dll", numErrors, numWarnings) + $"\nError: {firstError}");
            }

            //LocalizationLoader.HandleModBuilt(mod.Name);
        }
        catch (Exception e)
        {
            e.Data["mod"] = mod.Name;
            throw;
        }
    }
}
