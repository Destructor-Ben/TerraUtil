namespace TerraUtil.API.Utilities;

public static partial class Util
{
    /// <summary>
    /// Requests an asset in the Assets folder.
    /// </summary>
    /// <typeparam name="T">The type of the asset.</typeparam>
    /// <param name="name">The name of the asset.</param>
    /// <param name="loadAsync">Whether the asset will be loaded asynchronously or immediately.</param>
    /// <param name="prefix">An override for the prefix for the texture path.</param>
    /// <returns>The asset at location <c>{ModName}/Assets/{<paramref name="name" />}</c>.</returns>
    public static Asset<T> GetAsset<T>(string name, bool loadAsync = true, string prefix = null) where T : class
    {
        prefix ??= $"{Mod.Name}/Assets";
        return ModContent.Request<T>($"{prefix.Replace('.', '/')}/{name.Replace('.', '/')}", loadAsync ? AssetRequestMode.AsyncLoad : AssetRequestMode.ImmediateLoad);
    }

    /// <summary>
    /// Requests a texture in the Assets/Textures folder.
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <param name="loadAsync">Whether the texture will be loaded asynchronously or immediately.</param>
    /// <param name="prefix">An override for the prefix for the texture path.</param>
    /// <returns>The texture at location <c>{ModName}/Assets/Textures/{<paramref name="name" />}</c>.</returns>
    public static Asset<Texture2D> GetTexture(string name, bool loadAsync = true, string prefix = null)
    {
        prefix ??= $"{Mod.Name}/Assets/Textures";
        return GetAsset<Texture2D>(name, loadAsync, prefix);
    }
}
