using Terraria.GameInput;
using TerraUtil.API.Utilities;

namespace TerraUtil.API.Keybinds;

public class KeybindSystem : TerraUtilLoader<Keybind>
{
    public override void AddContent(Keybind content)
    {
        if (Util.IsHeadless)
            return;

        content.Key = KeybindLoader.RegisterKeybind(Mod, content.Name, content.DefaultBinding);
    }
}

internal class KeybindPlayer : ModPlayer
{
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (Util.IsHeadless)
            return;

        foreach (var keybind in KeybindSystem.Content)
        {
            keybind.Process();
        }
    }
}
