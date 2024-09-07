using Microsoft.Build.Framework;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TerraUtil.BuildSystem.Tasks;

public class GenerateModSourcesSymlink : BaseTask
{
    /// <summary>
    /// The path to the mod's code.
    /// </summary>
    [Required]
    public string ProjectPath { get; set; } = string.Empty;

    /// <summary>
    /// The path to ModSources.
    /// </summary>
    [Required]
    public string ModSourcesPath { get; set; } = string.Empty;

    /// <summary>
    /// The internal name of the mod.
    /// </summary>
    [Required]
    public string ModInternalName { get; set; } = string.Empty;

    protected override void Run()
    {
        // Check if the mod is already in ModSources
        string targetPath = Path.Join(ModSourcesPath, ModInternalName);
        if (Directory.Exists(targetPath))
            return;

        // Symlink!
        Log.LogMessage(MessageImportance.High, "Symlinking mod to ModSources...");

        // Admin is needed to make symlink, if it fails, make sure the user knows
        try
        {
            Directory.CreateSymbolicLink(targetPath, ProjectPath);
        }
        catch
        {
            Log.LogError("Unable to create symlink, please make sure that your IDE is run in admin mode or you run dotnet build with admin privileges.\nOnce the symlink is created, admin is no longer required to build the mod.");
            throw;
        }
    }
}
