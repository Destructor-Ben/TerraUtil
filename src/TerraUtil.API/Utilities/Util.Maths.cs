namespace TerraUtil.Utilities;

public static partial class Util
{
    /// <summary>
    /// A scalar to convert velocity in pixels per tick to miles per hour.
    /// </summary>
    public const float PPTToMPH = 216000f / 42240f;

    /// <summary>
    /// A scalar to convert acceleration in pixels per tick per tick to miles per hour per second.
    /// </summary>
    public const float PPTPTToMPHPS = PPTToMPH * 60f; // TODO: this isn't correct? should be divide, but then it doesn't work

    /// <summary>
    /// Gets whether a position is located within a rectangle.
    /// </summary>
    /// <param name="rect">The <see cref="Rectangle" /> to check.</param>
    /// <param name="pos">The position to check.</param>
    /// <returns>Whether <paramref name="pos" /> is inside <paramref name="rect" />.</returns>
    public static bool Contains(this Rectangle rect, Vector2 pos)
    {
        return rect.Contains(pos.ToPoint());
    }

    /// <summary>
    /// Rounds the given value.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="nearest">The nearest value to be rounded to.</param>
    /// <returns>The given value rounded.</returns>
    public static float Round(float value, float nearest = 1f)
    {
        return value - value % nearest;
    }

    /// <summary>
    /// A smoothstep function that interpolates <paramref name="x" />.
    /// </summary>
    /// <param name="x">The value to interpolate.</param>
    /// <param name="edge0">The left edge of the function.</param>
    /// <param name="edge1">The right edge of the function.</param>
    /// <returns>A smoothed version of <paramref name="x" />.</returns>
    public static float Smoothstep(float x, float edge0 = 0f, float edge1 = 1f)
    {
        x = MathHelper.Clamp((x - edge0) / (edge1 - edge0), edge0, edge1);
        return x * x * (3f - 2f * x);
    }

    /// <inheritdoc cref="Smoothstep(float, float, float)" />
    public static float Smootherstep(float x, float edge0 = 0f, float edge1 = 1f)
    {
        x = MathHelper.Clamp((x - edge0) / (edge1 - edge0), edge0, edge1);
        return x * x * x * (x * (x * 6f - 15f) + 10f);
    }

    /// <inheritdoc cref="Smoothstep(float, float, float)" />
    public static float EaseIn(float x, float edge0 = 0f, float edge1 = 1f)
    {
        x = MathHelper.Clamp((x - edge0) / (edge1 - edge0), edge0, edge1);
        return 2 * x * x;
    }

    /// <inheritdoc cref="Smoothstep(float, float, float)" />
    public static float EaseOut(float x, float edge0 = 0f, float edge1 = 1f)
    {
        x = MathHelper.Clamp((x - edge0) / (edge1 - edge0), edge0, edge1);
        x -= 0.5f;
        return 2 * x * (edge1 - x) + 0.5f;
    }
}
