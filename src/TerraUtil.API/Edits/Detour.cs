namespace TerraUtil.Edits;

/// <summary>
/// An abstraction of a detour.
/// </summary>
public abstract class Detour : ILoadable
{
    /// <inheritdoc cref="ModType.Mod" />
    public Mod Mod { get; internal set; }

    public void Load(Mod mod)
    {
        Mod = mod;
        Apply();
    }

    public void Unload()
    {
        Mod = null;
    }

    /// <summary>
    /// This is where you will apply your detour in the usual way.
    /// </summary>
    public abstract void Apply();
}
