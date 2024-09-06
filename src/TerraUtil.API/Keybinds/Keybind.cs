using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;

namespace TerraUtil.Keybinds;

/// <summary>
/// Provides an abstraction for <see cref="ModKeybind" />.
/// </summary>
public abstract class Keybind : TerraUtilModType
{
    /// <summary>
    /// The <see cref="ModKeybind" /> tied to this <see cref="Keybind" />.
    /// </summary>
    public ModKeybind Key { get; internal set; }

    /// <summary>
    /// Whether <see cref="Key" /> is currently pressed.
    /// </summary>
    public bool Pressed => Key.Current;

    /// <summary>
    /// Whether <see cref="Key" /> was pressed last frame.
    /// </summary>
    public bool WasPressed => Key.Old;

    /// <summary>
    /// Whether <see cref="Key" /> was pressed down this frame.
    /// </summary>
    public bool JustPressed => Key.JustPressed;

    /// <summary>
    /// Whether <see cref="Key" /> was released this frame.
    /// </summary>
    public bool JustReleased => Key.JustReleased;

    /// <summary>
    /// The default <see cref="Keys" /> that this <see cref="Keybind" /> will be set to.
    /// </summary>
    public abstract Keys DefaultBinding { get; }

    /// <summary>
    /// Called in <see cref="ModPlayer.ProcessTriggers(TriggersSet)" /> to process the keybind.
    /// </summary>
    public abstract void Process();

    protected override void Register()
    {
        ModTypeLookup<Keybind>.Register(this);
    }

    public sealed override void SetupContent()
    {
        SetStaticDefaults();
    }
}
