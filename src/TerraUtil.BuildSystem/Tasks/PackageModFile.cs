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
    /// Assembly references.
    /// </summary>
    [Required]
    public ITaskItem[] ReferencePaths { get; set; } = [];

    /// <summary>
    /// Mod references.
    /// </summary>
    [Required]
    public ITaskItem[] ModReferences { get; set; } = [];

    /// <summary>
    /// The directory where the .csproj is.
    /// </summary>
    [Required]
    public string ProjectDirectory { get; set; } = string.Empty;

    /// <summary>
    /// The directory where the compiled mod assembly is (relative to <see cref="ProjectDirectory" />).
    /// </summary>
    [Required]
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// The assembly name of the mod.
    /// </summary>
    [Required]
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// The path to tModLoader's assembly file.
    /// </summary>
    [Required]
    public string TmlDllPath { get; set; } = string.Empty;

    /// <summary>
    /// The path where the .tmod file should be written to.
    /// </summary>
    public string OutputTmodPath { get; set; } = string.Empty;

    /// <summary>
    /// The mod properties. (Previously found in <c>build.txt</c>)
    /// </summary>
    [Required]
    public ITaskItem[] ModProperties { get; set; } = [];

    private static readonly IList<string> SourceExtensions = [".csproj", ".cs", ".sln"];
    private static readonly IList<string> IgnoredNugetPackages = ["tModLoader.CodeAssist", "TerraUtil.BuildSystem"];

    protected override void Run()
    {
        // Verify the .tmod path exists, and find it if it doesn't
        if (string.IsNullOrEmpty(OutputTmodPath))
            OutputTmodPath = SavePathLocator.FindSavePath(Log, TmlDllPath, AssemblyName);

        Log.LogMessage(MessageImportance.Normal, $"Using path for .tmod file: {OutputTmodPath}");

        // Check the dll exists
        string modDllName = Path.ChangeExtension(AssemblyName, ".dll");
        string modDllPath = Path.Combine(ProjectDirectory, OutputPath, modDllName);
        if (!File.Exists(modDllPath))
            throw new FileNotFoundException("Mod dll not found.", modDllPath);

        Log.LogMessage(MessageImportance.Normal, $"Found mod's dll file: {modDllPath}");

        // Load the mod properties from the .csproj or build.txt
        var modProperties = GetModProperties();
        Log.LogMessage(MessageImportance.Normal, $"Loaded build properties: {modProperties}");

        var tmlVersion = SavePathLocator.GetTmlVersion(TmlDllPath);
        var tmodFile = new ModFile(OutputTmodPath, AssemblyName, modProperties.Version, tmlVersion);

        // Add files to the .tmod file
        tmodFile.AddFile(modDllName, File.ReadAllBytes(modDllPath));
        AddAllReferences(tmodFile, modProperties);

        string modPdbPath = Path.ChangeExtension(modDllPath, ".pdb");
        string modPdbName = Path.ChangeExtension(modDllName, ".pdb");
        if (File.Exists(modPdbPath))
        {
            tmodFile.AddFile(modPdbName, File.ReadAllBytes(modPdbPath));
            modProperties.EacPath = modPdbPath;
        }

        tmodFile.AddFile("Info", modProperties.ToBytes(tmlVersion.ToString()));

        Log.LogMessage(MessageImportance.Normal, "Adding resources...");
        var resources = Directory.GetFiles(ProjectDirectory, "*", SearchOption.AllDirectories)
                                 .Where(res => !IgnoreResource(modProperties, res))
                                 .ToList();

        Parallel.ForEach(resources, resource => AddResource(tmodFile, resource));

        // Save the mod file to the output folder and the mods folder
        Log.LogMessage(MessageImportance.Normal, "Saving mod file...");
        try
        {
            string outputFolderPath = Path.Join(ProjectDirectory, OutputPath, $"{AssemblyName}.tmod");
            tmodFile.Save();
            tmodFile.Save(outputFolderPath);
        }
        catch (Exception e)
        {
            Log.LogError($"Failed to create .tmod file. Check that the mod isn't enabled if tModLoader is open.\nFull error: {e}");
            return;
        }

        // Enable the mod
        string? modsFolder = Path.GetDirectoryName(OutputTmodPath);
        if (modsFolder is null)
        {
            Log.LogWarning("Couldn't get directory from .tmod output path.");
            return;
        }

        EnableMod(AssemblyName, modsFolder);
    }

    private void AddAllReferences(ModFile tmodFile, BuildProperties modProperties)
    {
        var nugetReferences = GetNugetReferences();
        var modReferences = GetModReferences();
        var projectReferences = GetProjectReferences();

        // Assumes all dll references are under the mod's folder (at same level or in subfolders).
        // Letting dll references be anywhere would mean doing some weird filters on references,
        // or using a custom `<DllReference>` thing that would get translated to a `<Reference>`.
        var dllReferences = ReferencePaths.Where(x => x.GetMetadata("FullPath").StartsWith(ProjectDirectory, StringComparison.Ordinal)).ToList();
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
            tmodFile.AddFile(nugetName, File.ReadAllBytes(nugetFile));
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
            tmodFile.AddFile($"lib/{dllName}.dll", File.ReadAllBytes(dllPath));
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
            tmodFile.AddFile(outputPath, File.ReadAllBytes(dllPath));
            modProperties.AddDllReference(Path.GetFileNameWithoutExtension(dllPath));
        }

        foreach (var modReference in modReferences)
        {
            string modName = modReference.GetMetadata("Identity");
            string modVersion = modReference.GetMetadata("Version");
            string weakRef = modReference.GetMetadata("Weak");

            Log.LogMessage(MessageImportance.Normal, $"Adding mod reference with mod name {modName} [Weak: {weakRef}, Version: {modVersion}]");
            modProperties.AddModReference(modName, modVersion is null ? null : new Version(modVersion), string.Equals(weakRef, "true", StringComparison.OrdinalIgnoreCase));
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
        foreach (var referencePath in ReferencePaths)
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
            string weakRef = modReference.GetMetadata("Weak");
            bool isWeak = string.Equals(weakRef, "true", StringComparison.OrdinalIgnoreCase);

            if (modName.Length == 0)
                throw new Exception("A mod reference must have an identity (Include=\"ModName\"). It should match the internal name of the mod you are referencing.");

            Log.LogMessage(MessageImportance.Normal, $"{modName} [Weak: {isWeak}] - Found at: {modPath}");
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
        var properties = BuildProperties.Read(ModProperties);
        string descriptionFilePath = Path.Combine(ProjectDirectory, "description.txt");
        // TODO: .buildignore
        if (!File.Exists(descriptionFilePath))
        {
            Log.LogWarning("Mod description not found with path: " + descriptionFilePath);
            return properties;
        }

        properties.Description = File.ReadAllText(descriptionFilePath);

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
        string path = resourcePath[(ProjectDirectory.Length + 1)..];
        return properties.IgnoreFile(path)
            || path[0] == '.'
            || path.StartsWith("bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || path.StartsWith("obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || SourceExtensions.Contains(Path.GetExtension(resourcePath))
            || Path.GetFileName(resourcePath) == "Thumbs.db";
    }

    private void AddResource(ModFile modFile, string resourcePath)
    {
        string relativePath = resourcePath[(ProjectDirectory.Length + 1)..];

        Log.LogMessage(MessageImportance.Low, "Adding resource: {0}", relativePath);

        using var src = File.OpenRead(resourcePath);
        using var dst = new MemoryStream();

        if (!ContentConverters.Convert(ref relativePath, src, dst))
            src.CopyTo(dst);

        modFile.AddFile(relativePath, dst.ToArray());
    }
}
