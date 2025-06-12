using Terraria.ModLoader.UI;
using Terraria.UI;

namespace TerraUtil.API.Utilities;

// TODO: add ways to not trigger parent OnMouseHover, but still set IsMouseHovering so hover effect apply to parent but don't double sound effects and onhover, etc.
// TODO: also allow elements to "swallow" mouse inputs when an element is hovered over so ui layers beneath it doesn't get triggered
// https://github.com/blushiemagic/MagicStorage/blob/4e780b75bf9fb292926d847cac4387eb142bb098/Common/Systems/MagicUI.cs
// https://github.com/blushiemagic/MagicStorage/blob/4e780b75bf9fb292926d847cac4387eb142bb098/Edits/ItemSlotDetours.cs
public static partial class Util
{
    /// <summary>
    /// A <see cref="Vector2" /> representing the size of the screen.
    /// </summary>
    public static Vector2 ScreenSize => Main.ScreenSize.ToVector2();

    /// <summary>
    /// A <see cref="Vector2" /> representing the position of the screen.
    /// </summary>
    public static Vector2 ScreenPos => Main.screenPosition;

    /// <summary>
    /// A <see cref="Rectangle" /> representing the screen's position and size.
    /// </summary>
    public static Rectangle Screen => new((int)ScreenPos.X, (int)ScreenPos.Y, (int)ScreenSize.X, (int)ScreenSize.Y);

    /// <summary>
    /// A <see cref="Vector2" /> representing the centre of the screen.
    /// </summary>
    public static Vector2 ScreenCenter => ScreenSize / 2f;

    /// <summary>
    /// A <see cref="Vector2" /> representing the centre of the screen in world coordinates.
    /// </summary>
    public static Vector2 ScreenWorldCenter => Screen.Center.ToVector2();

    /// <summary>
    /// The position of the mouse on the screen.
    /// </summary>
    public static Vector2 MousePos => Main.MouseScreen;

    /// <summary>
    /// The position of the mouse in the world.
    /// </summary>
    public static Vector2 MouseWorld => Main.MouseWorld;

    /// <summary>
    /// If the mouse was just left clicked.
    /// </summary>
    public static bool LeftClick => Main.mouseLeft && Main.mouseLeftRelease;

    /// <summary>
    /// If the mouse was just right clicked.
    /// </summary>
    public static bool RightClick => Main.mouseRight && Main.mouseRightRelease;

    /// <summary>
    /// If the mouse was just middle clicked.
    /// </summary>
    public static bool MiddleClick => Main.mouseMiddle && Main.mouseMiddleRelease;

    /// <summary>
    /// If the mouse was just X1 clicked.
    /// </summary>
    public static bool X1Click => Main.mouseXButton1 && Main.mouseXButton1Release;

    /// <summary>
    /// If the mouse was just X2 clicked.
    /// </summary>
    public static bool X2Click => Main.mouseXButton2 && Main.mouseXButton2Release;

    /// <summary>
    /// Sets the mouse text.
    /// </summary>
    /// <param name="text">The text to display as a tooltip.</param>
    /// <param name="tooltip">Whether a background should be shown for the tooltip.</param>
    public static void MouseText(string text, bool tooltip = false)
    {
        if (tooltip)
            UICommon.TooltipMouseText(text);
        else
            Main.instance.MouseText(text);
    }

    /// <summary>
    /// Resets all mouse text for this frame.
    /// </summary>
    public static void ResetMouseText()
    {
        Main.LocalPlayer.cursorItemIconEnabled = false;
        Main.LocalPlayer.cursorItemIconID = 0;
        Main.LocalPlayer.cursorItemIconText = string.Empty;
        Main.LocalPlayer.cursorItemIconPush = 0;
        Main.signHover = -1;
        Main.mouseText = false;
    }

    /// <summary>
    /// Makes the specified element display the specified mouse text when hovered over.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="element" />.</typeparam>
    /// <param name="element">The <see cref="UIElement" /> to display the hover text when hovering over.</param>
    /// <param name="text">The hover text to display.</param>
    /// <param name="tooltip">Whether a tooltip background should be displayed.</param>
    /// <returns><paramref name="element" /> to allow for chaining.</returns>
    public static T WithHoverText<T>(this T element, string text, bool tooltip = false) where T : UIElement
    {
        element.OnUpdate += delegate(UIElement affectedElement)
        {
            if (element.IsMouseHovering)
                MouseText(text, tooltip);
        };

        return element;
    }

    /// <summary>
    /// Makes the specified element play sounds when clicked or hovered over.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="element" />.</typeparam>
    /// <param name="element">The <see cref="UIElement" /> to play hover sounds when interacted with.</param>
    /// <param name="hoverSound">The <see cref="SoundStyle" /> to play when this element is hovered over.</param>
    /// <param name="clickSound">The <see cref="SoundStyle" /> to play when this element is left clicked.</param>
    /// <returns><paramref name="element" /> to allow for chaining.</returns>
    public static T WithHoverSounds<T>(this T element, SoundStyle? hoverSound = null, SoundStyle? clickSound = null) where T : UIElement
    {
        // Hover sound
        element.OnMouseOver += delegate(UIMouseEvent evt, UIElement listeningElement)
        {
            if (hoverSound != null)
                SoundEngine.PlaySound(hoverSound.Value);
        };

        // Click sound
        element.OnLeftClick += delegate(UIMouseEvent evt, UIElement listeningElement)
        {
            if (clickSound != null)
                SoundEngine.PlaySound(clickSound.Value);
        };

        return element;
    }
}
