namespace TerraUtil;

/// <summary>
/// A template <see cref="ModSystem" /> for loading content.
/// </summary>
/// <typeparam name="T">The type of the content being handled by this loader.</typeparam>
public abstract class TerraUtilLoader<T> : ModSystem where T : IModType, ILoadable
{
    public static IList<T> Content;

    /// <summary>
    /// The instance of this loader.
    /// </summary>
    public static TerraUtilLoader<T> Instance => ModContent.GetInstance<TerraUtilLoader<T>>();

    /// <inheritdoc cref="ModType.Mod" />
    public static new Mod Mod => Util.Mod;

    public override void Load()
    {
        Content = Mod.GetContent<T>().ToList();

        foreach (var content in Content)
        {
            AddContent(content);
        }
    }

    /// <summary>
    /// Called when the given content is registered.
    /// </summary>
    /// <param name="content">The content to register.</param>
    public virtual void AddContent(T content) { }
}
