using System.Data;
using Microsoft.Build.Framework;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace TerraUtil.BuildSystem.Core;

public class BuildProperties
{
    public string Author = string.Empty;
    public Version Version = new(1, 0);
    public string DisplayName = string.Empty;
    public string Homepage = string.Empty;
    public string Description = string.Empty;

    public List<string> DllReferences = [];
    public List<ModReference> ModReferences = [];
    public List<ModReference> WeakReferences = [];
    public string[] SortAfter = [];
    public string[] SortBefore = [];

    public Ignore.Ignore IgnoredFiles = new();
    public bool HideResources = false;
    public bool IncludeSource = false;
    public bool PlayableOnPreview = true;
    public bool TranslationMod = false;
    public string EacPath = string.Empty;
    public ModSide Side = ModSide.Both;

    public IEnumerable<ModReference> Refs(bool includeWeak)
    {
        return includeWeak ? ModReferences.Concat(WeakReferences) : ModReferences;
    }

    public IEnumerable<string> RefNames(bool includeWeak)
    {
        return Refs(includeWeak).Select(dep => dep.Mod);
    }

    public static void VerifyRefs(List<string> refs)
    {
        if (refs.Distinct().Count() != refs.Count)
            throw new DuplicateNameException("Duplicate mod or weak references.");
    }

    // Gets mod references and weak references that are not in sortBefore
    // Used to add them to sortAfter
    public string[] GetDistinctRefs()
    {
        return RefNames(true)
               .Where(m => !SortBefore.Contains(m))
               .Concat(SortAfter)
               .Distinct()
               .ToArray();
    }

    public static void WriteList<T>(IEnumerable<T> list, BinaryWriter writer)
    {
        foreach (var item in list)
        {
            writer.Write(item!.ToString()!);
        }

        writer.Write("");
    }

    public static BuildProperties Read(IEnumerable<ITaskItem> taskItems, List<string>? buildIgnore = null)
    {
        var properties = new BuildProperties();

        foreach (var property in taskItems)
        {
            string propertyName = property.ItemSpec;
            string propertyValue = property.GetMetadata("Value");
            ProcessProperty(properties, propertyName, propertyValue);
        }

        if (buildIgnore is not null)
        {
            foreach (string line in buildIgnore)
            {
                properties.IgnoredFiles.Add(line);
            }
        }

        VerifyRefs(properties.RefNames(true).ToList());
        properties.SortAfter = properties.GetDistinctRefs();

        return properties;
    }

    private static void ProcessProperty(BuildProperties properties, string property, string value)
    {
        switch (property)
        {
            // TODO: sortBefore, sortAfter
            case "Author":
                properties.Author = value;
                break;
            case "Version":
                properties.Version = new Version(value);
                break;
            case "DisplayName":
                properties.DisplayName = value;
                break;
            case "Homepage":
                properties.Homepage = value;
                break;
            case "HideResources":
                properties.HideResources = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                break;
            case "IncludeSource":
                properties.IncludeSource = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                break;
            case "PlayableOnPreview":
                properties.PlayableOnPreview = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                break;
            case "TranslationMod":
                properties.TranslationMod = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                break;
            case "Side":
                if (!Enum.TryParse(value, true, out properties.Side))
                    throw new ArgumentException("Side is not one of (Both, Client, Server, NoSync): " + value);

                break;
        }
    }

    public void AddDllReference(string name)
    {
        DllReferences.Add(name);
    }

    public void AddModReference(string modName, Version? modVersion, bool weak)
    {
        if (weak)
            WeakReferences.Add(new ModReference(modName, modVersion));
        else
            ModReferences.Add(new ModReference(modName, modVersion));
    }

    public byte[] ToBytes(string buildVersion)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        if (DllReferences.Count > 0)
        {
            writer.Write("dllReferences");
            WriteList(DllReferences, writer);
        }

        if (ModReferences.Count > 0)
        {
            writer.Write("modReferences");
            WriteList(ModReferences, writer);
        }

        if (WeakReferences.Count > 0)
        {
            writer.Write("weakReferences");
            WriteList(WeakReferences, writer);
        }

        if (SortAfter.Length > 0)
        {
            writer.Write("sortAfter");
            WriteList(SortAfter, writer);
        }

        if (SortBefore.Length > 0)
        {
            writer.Write("sortBefore");
            WriteList(SortBefore, writer);
        }

        if (Author.Length > 0)
        {
            writer.Write("author");
            writer.Write(Author);
        }

        writer.Write("version");
        writer.Write(Version.ToString());

        if (DisplayName.Length > 0)
        {
            writer.Write("displayName");
            writer.Write(DisplayName);
        }

        if (Homepage.Length > 0)
        {
            writer.Write("homepage");
            writer.Write(Homepage);
        }

        if (Description.Length > 0)
        {
            writer.Write("description");
            writer.Write(Description);
        }

        if (!HideResources)
            writer.Write("!hideResources");

        if (IncludeSource)
            writer.Write("includeSource");

        if (!PlayableOnPreview)
            writer.Write("!playableOnPreview");

        if (TranslationMod)
            writer.Write("translationMod");

        if (EacPath.Length > 0)
        {
            writer.Write("eacPath");
            writer.Write(EacPath);
        }

        if (Side != ModSide.Both)
        {
            writer.Write("side");
            writer.Write((byte)Side);
        }

        writer.Write("buildVersion");
        writer.Write(buildVersion);

        writer.Write("");
        return memoryStream.ToArray();
    }

    public bool IgnoreFile(string resource)
    {
        // Ignore dll references that would already be added by AddDllReference
        return IgnoredFiles.IsIgnored(resource) || DllReferences.Contains("lib/" + Path.GetFileName(resource));
    }
}
