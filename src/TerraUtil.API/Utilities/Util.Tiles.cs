namespace TerraUtil.API.Utilities;

public static partial class Util
{
    /// <summary>
    /// Retrieves the tile at the location of <paramref name="point" /> in tile coordinates.
    /// </summary>
    /// <param name="point">The tile coordinates of the tile.</param>
    /// <returns>The <see cref="Tile" /> from <see cref="Main.tile" /> at the location of <paramref name="point" />.</returns>
    public static Tile ToTile(this Point point)
    {
        return Main.tile[point];
    }

    /// <summary>
    /// Retrieves the tile at the location of <paramref name="vector" /> in world coordinates.
    /// </summary>
    /// <param name="vector">The world coordinates of the tile.</param>
    /// <returns>The <see cref="Tile" /> from <see cref="Main.tile" /> at the location of <paramref name="vector" />.</returns>
    public static Tile ToTile(this Vector2 vector)
    {
        return ToTile(vector.ToTileCoordinates());
    }
}
