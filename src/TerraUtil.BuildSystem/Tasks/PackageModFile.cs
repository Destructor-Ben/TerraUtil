using Microsoft.Build.Framework;
using Newtonsoft.Json;
using TerraUtil.BuildSystem.Core;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace TerraUtil.BuildSystem.Tasks;

public class PackageModFile : BaseTask
{
    /// <summary>
    /// NuGet package references.
    /// </summary>
    [Required]
    public ITaskItem[] PackageReferences { get; set; } = [];

    /// <summary>
    /// Project references.
    /// </summary>
    public ITaskItem[] ProjectReferences { get; set; } = [];

    /// <summary>
    /// Dll references.
    /// </summary>
    [Required]
    public ITaskItem[] DllReferences { get; set; } = [];

    /// <summary>
    /// Mod references.
    /// </summary>
    [Required]
    public ITaskItem[] ModReferences { get; set; } = [];

    /// <summary>
    /// The directory where the .csproj is.
    /// </summary>
    [Required]
    public string ProjectPath { get; set; } = string.Empty;

    /// <summary>
    /// The directory where the compiled mod assembly is (relative to <see cref="ProjectPath" />).
    /// </summary>
    [Required]
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// The internal name (and assembly name) of the mod.
    /// </summary>
    [Required]
    public string InternalName { get; set; } = string.Empty;

    /// <summary>
    /// The path to tModLoader's assembly file.
    /// </summary>
    [Required]
    public string ModLoaderDllPath { get; set; } = string.Empty;

    /// <summary>
    /// The path where the .tmod file should be written to. Optional, can be set by the modder.
    /// </summary>
    public string OutputModFilePath { get; set; } = string.Empty;

    /// <summary>
    /// The mod properties. (Previously found in <c>build.txt</c>)
    /// </summary>
    [Required]
    public ITaskItem[] ModProperties { get; set; } = [];

    /// <summary>
    /// The list of mods that this mod needs to load before.
    /// </summary>
    [Required]
    public ITaskItem[] SortBefore { get; set; } = [];

    /// <summary>
    /// The list of mods that this mod needs to load after.
    /// </summary>
    [Required]
    public ITaskItem[] SortAfter { get; set; } = [];

    private static readonly IList<string> IgnoredNugetPackages = ["tModLoader.CodeAssist", "TerraUtil.BuildSystem"];

    protected override void Run()
    {
        // Verify the .tmod path exists, and find it if it doesn't
        if (string.IsNullOrEmpty(OutputModFilePath))
            OutputModFilePath = SavePathLocator.FindSavePath(Log, ModLoaderDllPath, InternalName);

        Log.LogMessage(MessageImportance.Normal, $"Using path for .tmod file: {OutputModFilePath}");

        // Check the dll exists
        string modDllName = InternalName + ".dll";
        string modDllPath = Path.Combine(ProjectPath, OutputPath, modDllName);
        if (!File.Exists(modDllPath))
            throw new FileNotFoundException("Mod dll not found.", modDllPath);

        Log.LogMessage(MessageImportance.Normal, $"Found mod's dll file: {modDllPath}");

        // Load the mod properties
        var modProperties = GetModProperties();
        Log.LogMessage(MessageImportance.Normal, $"Loaded build properties: {modProperties}");

        // Create the mod file
        var tmlVersion = SavePathLocator.GetTmlVersion(ModLoaderDllPath);
        var modFile = new ModFile(OutputModFilePath, InternalName, modProperties.Version, tmlVersion);

        // Add dlls to the .tmod file
        modFile.AddFile(modDllName, File.ReadAllBytes(modDllPath));
        AddAllReferences(modFile, modProperties);

        // Add the PDB if it exists
        string modPdbPath = Path.ChangeExtension(modDllPath, ".pdb");
        string modPdbName = Path.ChangeExtension(modDllName, ".pdb");
        if (File.Exists(modPdbPath))
        {
            modFile.AddFile(modPdbName, File.ReadAllBytes(modPdbPath));
            modProperties.EacPath = modPdbPath;
        }

        // Add the mod properties
        modFile.AddFile("Info", modProperties.ToBytes(tmlVersion.ToString()));

        // Add the resources
        Log.LogMessage(MessageImportance.Normal, "Adding resources...");
        var resources = Directory.GetFiles(ProjectPath, "*", SearchOption.AllDirectories)
                                 .Where(res => !IgnoreResource(modProperties, res))
                                 .ToList();

        Parallel.ForEach(resources, resource => AddResource(modFile, resource));

        // Save the mod file to the output folder and the mods folder
        Log.LogMessage(MessageImportance.Normal, "Saving mod file...");
        try
        {
            string outputFolderPath = Path.Join(ProjectPath, OutputPath, $"{InternalName}.tmod");
            modFile.Save();
            modFile.Save(outputFolderPath);
        }
        catch (Exception e)
        {
            Log.LogError($"Failed to create .tmod file. Check that the mod isn't enabled if tModLoader is open.\nFull error: {e}");
            return;
        }

        // Enable the mod
        string? modsFolder = Path.GetDirectoryName(OutputModFilePath);
        if (modsFolder is null)
        {
            Log.LogWarning("Couldn't get directory from .tmod output path.");
            return;
        }

        EnableMod(InternalName, modsFolder);
    }

    private void AddAllReferences(ModFile modFile, BuildProperties modProperties)
    {
        var nugetReferences = GetNugetReferences();
        var modReferences = GetModReferences();
        var projectReferences = GetProjectReferences();

        // Assumes all dll references are under the mod's folder (at same level or in subfolders).
        // Letting dll references be anywhere would mean doing some weird filters on references,
        // or using a custom `<DllReference>` thing that would get translated to a `<Reference>`.
        var dllReferences = DllReferences.Where(x => x.GetMetadata("FullPath").StartsWith(ProjectPath, StringComparison.Ordinal)).ToList();
        Log.LogMessage(MessageImportance.Normal, $"Found {dllReferences.Count} dll references.");

        foreach (var taskItem in nugetReferences)
        {
            string nugetName = "lib/" + taskItem.GetMetadata("NuGetPackageId") + ".dll";
            string nugetFile = taskItem.GetMetadata("HintPath");

            if (string.Equals(taskItem.GetMetadata("Private"), "true", StringComparison.OrdinalIgnoreCase))
            {
                Log.LogMessage(MessageImportance.Normal, $"Skipping private reference: {nugetName}");
                continue;
            }

            Log.LogMessage(MessageImportance.Normal, $"Adding nuget {nugetName} with path {nugetFile}");
            modFile.AddFile(nugetName, File.ReadAllBytes(nugetFile));
            modProperties.AddDllReference(taskItem.GetMetadata("NuGetPackageId"));
        }

        foreach (var dllReference in dllReferences)
        {
            string dllPath = dllReference.GetMetadata("FullPath");
            string dllName = Path.GetFileNameWithoutExtension(dllPath);

            if (string.Equals(dllReference.GetMetadata("Private"), "true", StringComparison.OrdinalIgnoreCase))
            {
                Log.LogMessage(MessageImportance.Normal, $"Skipping private reference: {dllName}");
                continue;
            }

            Log.LogMessage(MessageImportance.Normal, $"Adding dll reference with path {dllPath}");
            modFile.AddFile($"lib/{dllName}.dll", File.ReadAllBytes(dllPath));
            modProperties.AddDllReference(dllName);
        }

        foreach (var projectReference in projectReferences)
        {
            string dllPath = projectReference.ItemSpec;
            string outputPath = "lib/" + Path.GetFileName(dllPath);
            string csprojPath = projectReference.GetMetadata("OriginalItemSpec"); // Path to .csproj

            if (string.Equals(projectReference.GetMetadata("Private"), "true", StringComparison.OrdinalIgnoreCase))
            {
                Log.LogMessage(MessageImportance.Normal, $"Skipping private project reference: {csprojPath}");
                continue;
            }

            Log.LogMessage(MessageImportance.Normal, $"Adding project reference with path {csprojPath}");
            modFile.AddFile(outputPath, File.ReadAllBytes(dllPath));
            modProperties.AddDllReference(Path.GetFileNameWithoutExtension(dllPath));
        }

        foreach (var modReference in modReferences)
        {
            string modName = modReference.GetMetadata("Identity");
            string modVersion = modReference.GetMetadata("Version");
            string weakRef = modReference.GetMetadata("Weak");

            Log.LogMessage(MessageImportance.Normal, $"Adding mod reference with mod name {modName} [Weak: {weakRef}, Version: {modVersion}]");
            modProperties.AddModReference(modName, string.IsNullOrWhiteSpace(modVersion) ? null : new Version(modVersion), string.Equals(weakRef, "true", StringComparison.OrdinalIgnoreCase));
        }

        // Add modReferences to sortAfter if they are not already in sortBefore
        modProperties.SortAfter = modProperties.GetDistinctRefs();
    }

    private List<ITaskItem> GetNugetReferences()
    {
        var nugetLookup = PackageReferences.ToDictionary(x => x.ItemSpec);

        // Check if any packages in IgnoredNugetPackages are present in nugetLookup, and if they are, remove them
        foreach (string ignoredNugetPackage in IgnoredNugetPackages)
        {
            Log.LogWarning("Ignoring package: " + ignoredNugetPackage);
            if (nugetLookup.ContainsKey(ignoredNugetPackage))
            {
                Log.LogWarning("Ignored package: " + ignoredNugetPackage);
                Log.LogMessage(MessageImportance.Normal, $"Ignoring nuget package: {ignoredNugetPackage}");
                nugetLookup.Remove(ignoredNugetPackage);
            }
        }

        List<ITaskItem> nugetReferences = [];
        foreach (var referencePath in DllReferences)
        {
            string? hintPath = referencePath.GetMetadata("HintPath");
            string? nugetPackageId = referencePath.GetMetadata("NuGetPackageId");
            string? nugetPackageVersion = referencePath.GetMetadata("NuGetPackageVersion");

            if (string.IsNullOrEmpty(nugetPackageId))
                continue;

            if (!nugetLookup.TryGetValue(nugetPackageId, out var nugetItem))
                continue;

            // Copy Private metadata from the <PackageReference> to the item containing the reference path
            referencePath.SetMetadata("Private", nugetItem.GetMetadata("Private"));

            Log.LogMessage(MessageImportance.Normal, $"{nugetPackageId} - v{nugetPackageVersion} - Found at: {hintPath}");
            nugetReferences.Add(referencePath);
        }

        Log.LogMessage(MessageImportance.Normal, $"Found {nugetReferences.Count} nuget references.");

        if (nugetLookup.Count != nugetReferences.Count)
            Log.LogWarning($"Expected {nugetLookup.Count} nuget references but found {nugetReferences.Count}.");

        return nugetReferences;
    }

    private List<ITaskItem> GetModReferences()
    {
        List<ITaskItem> modReferences = [];
        foreach (var modReference in ModReferences)
        {
            string modPath = modReference.GetMetadata("HintPath");
            if (modPath.Length == 0)
                modPath = modReference.GetMetadata("ProjectPath");

            string modName = modReference.GetMetadata("Identity");
            string modVersion = modReference.GetMetadata("Version");
            string weakRef = modReference.GetMetadata("Weak");
            bool isWeak = string.Equals(weakRef, "true", StringComparison.OrdinalIgnoreCase);

            if (modName.Length == 0)
                throw new Exception("A mod reference must have an identity (Include=\"ModName\"). It should match the internal name of the mod you are referencing.");

            Log.LogMessage(MessageImportance.Normal, $"{modName} [Weak: {isWeak}, Version: {modVersion}] - Found at: {modPath}");
            modReferences.Add(modReference);
        }

        Log.LogMessage(MessageImportance.Normal, $"Found {modReferences.Count} mod references.");
        return modReferences;
    }

    private List<ITaskItem> GetProjectReferences()
    {
        List<ITaskItem> projectReferences = [];
        foreach (var projectReference in ProjectReferences)
        {
            string dllPath = projectReference.ItemSpec;
            if (!File.Exists(dllPath))
            {
                Log.LogWarning("Project reference dll not found: " + dllPath);
                continue;
            }

            projectReferences.Add(projectReference);
        }

        return projectReferences;
    }

    private BuildProperties GetModProperties()
    {
        var properties = BuildProperties.Read(ModProperties, SortBefore, SortAfter);

        // Get the description
        string descriptionPath = Path.Combine(ProjectPath, "description.txt");
        if (File.Exists(descriptionPath))
            properties.Description = File.ReadAllText(descriptionPath);
        else
            Log.LogWarning("Mod description not found with path: " + descriptionPath);

        // Add default ignored files
        // This should be done before the .buildignore so it can be overriden
        properties.IgnoredFiles.Add(
            [
                ".*",
                "bin/",
                "obj/",
                "*.csproj",
                "*.sln",
                "*.cs",
                "Properties/launchSettings.json",
                "Thumbs.db",
            ]
        );

        // Get the list of ignored files
        string buildIgnoreFilePath = Path.Combine(ProjectPath, ".buildignore");
        if (File.Exists(buildIgnoreFilePath))
            properties.IgnoredFiles.Add(File.ReadLines(buildIgnoreFilePath));
        else
            Log.LogMessage(MessageImportance.High, ".buildignore not found with path: " + buildIgnoreFilePath);

        return properties;
    }

    private void EnableMod(string modName, string modsFolderPath)
    {
        string enabledPath = Path.Combine(modsFolderPath, "enabled.json");
        if (!File.Exists(enabledPath))
        {
            Log.LogMessage(MessageImportance.Low, $"enabled.json not found at '{enabledPath}', the mod will not be enabled.");
            return;
        }

        string enabledJson = File.ReadAllText(enabledPath);
        try
        {
            var enabled = JsonConvert.DeserializeObject<List<string>>(enabledJson);
            if (enabled!.Contains(modName))
                return;

            enabled.Add(modName);
            File.WriteAllText(enabledPath, JsonConvert.SerializeObject(enabled, Formatting.Indented));
        }
        catch (Exception e)
        {
            Log.LogWarning($"Failed to enable mod {modName}: {e}");
        }
    }

    private bool IgnoreResource(BuildProperties properties, string resourcePath)
    {
        // Path relative to the project path
        string path = resourcePath[(ProjectPath.Length + 1)..];
        return properties.IgnoreFile(path);
    }

    private void AddResource(ModFile modFile, string resourcePath)
    {
        string relativePath = resourcePath[(ProjectPath.Length + 1)..];

        Log.LogMessage(MessageImportance.Low, "Adding resource: {0}", relativePath);

        using var src = File.OpenRead(resourcePath);
        using var dst = new MemoryStream();

        if (!ContentConverters.Convert(ref relativePath, src, dst))
            src.CopyTo(dst);

        modFile.AddFile(relativePath, dst.ToArray());
    }
}
