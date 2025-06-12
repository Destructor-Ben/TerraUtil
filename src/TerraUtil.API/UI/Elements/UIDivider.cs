using Terraria.UI;

namespace TerraUtil.API.UI.Elements;

public class UIDivider : UIElement
{
    public bool Horizontal;

    public UIDivider(bool horizontal = true)
    {
        Horizontal = horizontal;

        if (Horizontal)
        {
            Width.Set(0, 1f);
            Height.Set(4, 0f);
        }
        else
        {
            Width.Set(4, 0f);
            Height.Set(0, 1f);
        }
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var dimensions = GetDimensions();

        var mainRect = Horizontal ? new((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, 2) : new Rectangle((int)dimensions.X, (int)dimensions.Y, 2, (int)dimensions.Height);
        var secondaryRect = Horizontal ? new((int)dimensions.X, (int)dimensions.Y + 2, (int)dimensions.Width, 2) : new Rectangle((int)dimensions.X + 2, (int)dimensions.Y, 2, (int)dimensions.Height);

        spriteBatch.Draw(TextureAssets.MagicPixel.Value, mainRect, Color.LightGray);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, secondaryRect, new(0.4f, 0.4f, 0.4f)); // TODO: colour
    }
}
