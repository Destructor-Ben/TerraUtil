using TerraUtil.API.Utilities;

namespace TerraUtil.API;

/// <summary>
/// A template <see cref="ModType" /> used by TerraUtil.
/// </summary>
public abstract class TerraUtilModType : ModType
{
    /// <inheritdoc cref="ModType.Mod" />
    public static new Mod Mod => Util.Mod;
}
