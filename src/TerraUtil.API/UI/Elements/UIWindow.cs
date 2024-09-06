using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace TerraUtil.UI.Elements;

public abstract class UIWindow : Interface
{
    private UIImageButton CloseButton;
    private bool dragging;

    private Vector2 dragOffset;
    private UIElement TitleBar;
    protected abstract LocalizedText WindowTitle { get; }
    protected virtual Vector2 Size { get; set; } = new(900, 600);
    protected virtual Vector2 InitialPosition { get; set; } = new(0.5f, 0.5f);

    public UIPanel Panel { get; private set; }
    public UIElement Content { get; private set; }

    public override int GetLayerInsertIndex(List<GameInterfaceLayer> layers)
    {
        return layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
    }

    protected override void CreateUI()
    {
        // Background
        Panel = new()
        {
            Width = { Pixels = Size.X },
            Height = { Pixels = Size.Y },
            HAlign = InitialPosition.X,
            VAlign = InitialPosition.Y,
            PaddingTop = 6,
            BackgroundColor = UICommon.MainPanelBackground
        };

        Append(Panel);

        // Title bar
        TitleBar = new()
        {
            Width = { Percent = 1f },
            Height = { Pixels = 30 }
        };

        Panel.Append(TitleBar);

        // Content
        Content = new()
        {
            Width = { Percent = 1f },
            Height =
            {
                Pixels = -30,
                Percent = 1f
            },
            Top = { Pixels = 30 },
            PaddingTop = 12
        };

        Panel.Append(Content);

        // Close button
        CloseButton = new(Util.GetTexture("SearchCancel", false, "Terraria.Images.UI"))
        {
            VAlign = 0f,
            HAlign = 1f,
            MarginRight = -6
        };

        CloseButton.OnLeftClick += (_, _) =>
        {
            Visible = false;
            SoundEngine.PlaySound(SoundID.MenuClose);
        };

        TitleBar.Append(CloseButton);

        // Title
        var title = new UIText(WindowTitle, 0.5f, true) { VAlign = 0.5f };
        TitleBar.Append(title);

        // Divider
        var divider = new UIDivider { VAlign = 1f };
        TitleBar.Append(divider);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Stop weapons from being able to be used while the window is being hovered over
        // TODO: move this to be included with all UI
        if (Panel.IsMouseHovering && Visible)
            Main.LocalPlayer.mouseInterface = true;

        // Dragging
        var dimensions = GetDimensions();

        if (!Main.mouseLeft)
            dragging = false;

        // TODO: only start dragging on left click
        if (dragging || (Main.mouseLeft && TitleBar.ContainsPoint(Main.MouseScreen) && !CloseButton.ContainsPoint(Main.MouseScreen)))
        {
            dragging = true;

            if (dragOffset == Vector2.Zero)
                dragOffset = Main.MouseScreen - dimensions.Position();

            var newPos = Main.MouseScreen - dragOffset;
            Left.Set(newPos.X, 0f);
            Top.Set(newPos.Y, 0f);
            HAlign = 0f;
            VAlign = 0f;
        }
        else
        {
            dragOffset = Vector2.Zero;
        }
    }
}
