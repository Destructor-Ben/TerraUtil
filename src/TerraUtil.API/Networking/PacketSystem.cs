using TerraUtil.API.Utilities;

namespace TerraUtil.API.Networking;

public class PacketSystem : TerraUtilLoader<Packet>
{
    private static int idCounter = 0;

    public override void Load()
    {
        idCounter = 0;
    }

    public override void AddContent(Packet content)
    {
        content.Type = idCounter;
        idCounter++;
    }

    /// <summary>
    /// Call this in <see cref="Mod.HandlePacket(BinaryReader, int)" /> to enable custom packets.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader" /> used to read the data.</param>
    /// <param name="sender">The sender of the packet.</param>
    public static void HandlePacket(BinaryReader reader, int sender)
    {
        int? fromWho = Util.MachineID(sender);
        int id = reader.ReadInt32();

        var packet = Content[id];
        packet.Deserialize(reader);

        // Handling for different send types
        var sendType = (Packet.SendType)reader.ReadByte();
        switch (sendType)
        {
            case Packet.SendType.SendToServer:
                packet.HandleSendToServer(fromWho);
                break;
            case Packet.SendType.SendToClient:
                packet.HandleSendToClient(reader, fromWho);
                break;
            case Packet.SendType.SendToClients:
                packet.HandleSendToClients(reader, fromWho);
                break;
            case Packet.SendType.SendToAllClients:
                packet.HandleSendToAllClients(fromWho);
                break;
            case Packet.SendType.SendToAll:
                packet.HandleSendToAll(fromWho);
                break;
            default:
                Util.Mod.Logger.Warn("Unknown packet type: " + (byte)sendType);
                break;
        }
    }
}
