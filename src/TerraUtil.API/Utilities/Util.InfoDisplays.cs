namespace TerraUtil.Utilities;

public static partial class Util
{
    /// <summary>
    /// Returns if an info display is active and not hidden.
    /// </summary>
    /// <param name="display">The display to check.</param>
    /// <returns>Whether <paramref name="display" /> is active.</returns>
    public static bool InfoDisplayActive(InfoDisplay display)
    {
        return display.Active() && !Main.LocalPlayer.hideInfo[display.Type];
    }
}
