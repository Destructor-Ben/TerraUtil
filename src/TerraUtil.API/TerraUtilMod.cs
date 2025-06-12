using TerraUtil.API.Networking;
using TerraUtil.API.Utilities;

namespace TerraUtil.API;

/// <summary>
/// Functions as the base <see cref="Mod" /> class but integrates TerraUtil loading into it.
/// </summary>
public class TerraUtilMod : Mod
{
    // Have to use constructor because Load isn't called before all other content
    public TerraUtilMod()
    {
        Util.Load(this);
    }

    public override void Load()
    {
        Util.LoadContent();
    }

    public override void Unload()
    {
        Util.Unload();
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        PacketSystem.HandlePacket(reader, whoAmI);
    }
}
