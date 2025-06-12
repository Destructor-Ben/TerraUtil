using System.Reflection;
using Terraria.ModLoader.Core;

namespace TerraUtil.API.Utilities;

/// <summary>
/// A static class containing many helper and utility fields and methods.
/// </summary>
public static partial class Util
{
    internal static Mod Mod { get; private set; }

    /// <summary>
    /// Call this in <see cref="Mod.Mod" /> to initialize TerraUtil.
    /// </summary>
    /// <param name="mod">The <see cref="Mod" /> instance loading TerraUtil.</param>
    public static void Load(Mod mod)
    {
        Mod = mod;
    }

    /// <summary>
    /// Call this in <see cref="Mod.Load" /> to add TerraUtil's content.
    /// </summary>
    public static void LoadContent()
    {
        // Autoload content
        if (!Mod.ContentAutoloadingEnabled)
            return;

        var loadableTypes = AssemblyManager.GetLoadableTypes(typeof(Util).Assembly)
                                           .Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
                                           .Where(t => t.IsAssignableTo(typeof(ILoadable)))
                                           .Where(t => t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) != null)
                                           .Where(t => AutoloadAttribute.GetValue(t).NeedsAutoloading)
                                           .OrderBy(type => type.FullName, StringComparer.InvariantCulture);

        LoaderUtils.ForEachAndAggregateExceptions(loadableTypes, t => Mod.AddContent((ILoadable)Activator.CreateInstance(t, true)));
    }

    /// <summary>
    /// Call this in <see cref="Mod.Unload" /> to uninitialize TerraUtil.
    /// </summary>
    public static void Unload()
    {
        Mod = null;
    }
}
