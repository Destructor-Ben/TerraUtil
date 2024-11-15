using Terraria.ModLoader;

namespace TerraUtilDevTools;

public class DevTools : Mod
{
    public static DevTools Instance => ModContent.GetInstance<DevTools>();

    public override void Load()
    {
        Logger.Debug("Hello!");
    }
}
