using Terraria.UI;

namespace TerraUtil.UI;

/// <summary>
/// Provides an abstraction for a <see cref="Terraria.UI.UserInterface" /> and <see cref="UIState" />.
/// </summary>
public abstract partial class Interface : UIState, ILoadable, IModType
{
    private bool _visible = true;

    /// <summary>
    /// The internal <see cref="Terraria.UI.UserInterface" /> that this <see cref="Interface" /> uses.
    /// </summary>
    public UserInterface UserInterface { get; internal set; }

    /// <summary>
    /// Whether <see cref="UserInterface" /> should draw.
    /// </summary>
    public virtual bool Visible
    {
        get => _visible;
        set
        {
            if (value)
                Activate();

            _visible = value;
        }
    }

    /// <summary>
    /// Whether <see cref="UserInterface" /> should update.
    /// </summary>
    public virtual bool ShouldUpdate { get; set; } = true;

    /// <summary>
    /// The <see cref="InterfaceScaleType" /> of <see cref="UserInterface" />.<br />
    /// By default is <see cref="InterfaceScaleType.UI" />.
    /// </summary>
    public virtual InterfaceScaleType ScaleType => InterfaceScaleType.UI;

    /// <summary>
    /// Returns the index that this <see cref="UserInterface" /> should be inserted in <paramref name="layers" />.<br />
    /// Return -1 if you don't want to insert it.
    /// </summary>
    /// <param name="layers">The <see cref="List{T}" /> of <see cref="GameInterfaceLayer" />s to be drawn.</param>
    /// <returns>The layer index to insert at.</returns>
    public abstract int GetLayerInsertIndex(List<GameInterfaceLayer> layers);

    /// <summary>
    /// Called when <see cref="UserInterface" /> is loaded.
    /// </summary>
    public virtual void Load() { }

    /// <summary>
    /// Called when <see cref="UserInterface" /> is unloaded.
    /// </summary>
    public virtual void Unload() { }

    public sealed override void OnActivate()
    {
        RemoveAllChildren();
        ResetUI();
        CreateUI();
        Recalculate();
    }

    /// <summary>
    /// Override this to initialie your UI and any data you might want to store.
    /// </summary>
    protected virtual void CreateUI() { }

    /// <summary>
    /// Override this to unload any data that you might have stored.<br />
    /// All UI has already been removed when this is called. Make sure to use null checks.
    /// </summary>
    protected virtual void ResetUI() { }

    #region Setup

    /// <inheritdoc cref="ModType.Mod" />
    public Mod Mod => Util.Mod;

    /// <inheritdoc cref="ModType.Name" />
    public virtual string Name => GetType().Name;

    /// <inheritdoc cref="ModType.FullName" />
    public string FullName => (Mod?.Name ?? "Terraria") + "/" + Name;

    void ILoadable.Load(Mod mod)
    {
        ModTypeLookup<Interface>.Register(this);
        Load();
    }

    void ILoadable.Unload()
    {
        ResetUI();
        Unload();
    }

    #endregion
}
