using Terraria.UI;
using TerraUtil.API.Utilities;

namespace TerraUtil.API.UI;

public class UISystem : TerraUtilLoader<Interface>
{
    public override void AddContent(Interface content)
    {
        if (Util.IsHeadless)
            return;

        content.UserInterface = new();
        content.UserInterface.SetState(content);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (Util.IsHeadless)
            return;

        foreach (var ui in Content)
        {
            int index = ui.GetLayerInsertIndex(layers);
            if (index == -1)
                return;

            layers.Insert(
                index,
                new LegacyGameInterfaceLayer(
                    Util.Mod.Name + ": " + ui.Name,
                    delegate
                    {
                        if (!ui.Visible)
                            return true;

                        ui.UserInterface?.Draw(Main.spriteBatch, null);
                        return true;
                    },
                    ui.ScaleType
                )
            );
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (Util.IsHeadless)
            return;

        foreach (var ui in Content)
        {
            if (ui.ShouldUpdate)
                ui.UserInterface?.Update(gameTime);
        }
    }
}
