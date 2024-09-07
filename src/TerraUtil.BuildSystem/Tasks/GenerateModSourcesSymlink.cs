﻿using Microsoft.Build.Framework;

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
        Log.LogMessage("Symlinking project to ModSources...");
        Directory.CreateSymbolicLink(targetPath, ProjectPath);
    }
}