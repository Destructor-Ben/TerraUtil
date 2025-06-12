using TerraUtil.API.Utilities;

namespace TerraUtil.API.Networking;

/// <summary>
/// Provides an abstraction for ModPackets.
/// </summary>
public abstract class Packet : TerraUtilModType
{
    #region Setup

    internal enum SendType : byte
    {
        SendToServer,
        SendToClient,
        SendToClients,
        SendToAllClients,
        SendToAll
    }

    public int Type { get; internal set; }

    protected sealed override void Register()
    {
        ModTypeLookup<Packet>.Register(this);
    }

    public sealed override void SetupContent()
    {
        SetStaticDefaults();
    }

    #endregion

    #region Public Stuff

    /// <summary>
    /// If true and the current machine is the target of a packet, the packet will also be handled on that machine.<br />
    /// Works in singleplayer.
    /// </summary>
    public virtual bool HandleIfTarget => true;

    /// <summary>
    /// Makes the packet do something when it has arrived.
    /// </summary>
    /// <param name="fromWho"><see langword="null" /> if from the server, otherwise the whoAmI of a client.</param>
    public abstract void Handle(int? fromWho);

    /// <summary>
    /// Called when the packet is about to be sent.
    /// </summary>
    /// <param name="toWho"><see langword="null" /> if to the server, otherwise the whoAmI of a client.</param>
    public virtual void OnSend(int? toWho) { }

    /// <summary>
    /// Serialize the packet data in this method.
    /// </summary>
    /// <param name="writer">The <see cref="BinaryWriter" /> used to write the data.</param>
    public abstract void Serialize(BinaryWriter writer);

    /// <summary>
    /// Deserialize the packet data in this method.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader" /> used to read the data.</param>
    public abstract void Deserialize(BinaryReader reader);

    #endregion

    #region Sending

    /// <summary>
    /// Gets a <see cref="ModPacket" /> with it's type and data already written to it.
    /// </summary>
    /// <returns>A <see cref="ModPacket" /> with it's type and data written to it in that order.</returns>
    internal ModPacket GetPacket()
    {
        if (Util.IsSingleplayer)
            return null;

        var packet = Mod.GetPacket();
        packet.Write(Type);
        Serialize(packet);
        return packet;
    }

    // Writes the specified sendtype to the packet and sends it
    // TODO: allow clients to see the sender of a packet
    internal void Send(ModPacket packet, SendType sendType, int toWho = -1, int ignoreWho = -1)
    {
        if (Util.IsSingleplayer)
            return;

        packet.Write((byte)sendType);
        OnSend(Util.IsClient ? null : Util.MachineID(toWho));
        packet.Send(toWho, ignoreWho);
    }

    /// <summary>
    /// Sends this packet to the server.
    /// </summary>
    public void SendToServer()
    {
        if (Util.IsServer)
        {
            // Server handles the packet itself
            HandleFromTarget();
        }
        else if (Util.IsClient)
        {
            // Send to server
            var packet = GetPacket();
            Send(packet, SendType.SendToServer);
        }
    }

    /// <summary>
    /// Sends this packet to a specific client.
    /// </summary>
    /// <param name="toWho">The whoAmI of the client to send this packet to.</param>
    public void SendToClient(int toWho)
    {
        if (Util.IsServer)
        {
            // Server sends the packet to the client
            var packet = GetPacket();
            Send(packet, SendType.SendToClient, toWho);
        }
        else if (Util.IsClient && toWho == Main.myPlayer)
        {
            HandleFromTarget();
        }
        else if (Util.IsClient)
        {
            // Send packet to server so it can forward it
            var packet = GetPacket();
            packet.Write(toWho);
            Send(packet, SendType.SendToClient);
        }
    }

    /// <summary>
    /// Sends this packet to the specified clients
    /// </summary>
    /// <param name="handleIfTarget"></param>
    /// <param name="toWho"></param>
    public void SendToClients(params int[] toWho)
    {
        if (Util.IsServer)
        {
            // Server sends packets to the clients
            foreach (int client in toWho)
            {
                var packet = GetPacket();
                Send(packet, SendType.SendToClients, client);
            }
        }
        else if (Util.IsClient && toWho.Contains(Main.myPlayer))
        {
            HandleFromTarget();
        }
        else if (Util.IsClient)
        {
            // Send packet to server so it can forward it
            var packet = GetPacket();
            packet.Write(toWho.Length);
            foreach (int client in toWho)
            {
                packet.Write(client);
            }

            Send(packet, SendType.SendToClients);
        }
    }

    /// <summary>
    /// Sends this packet to all clients
    /// </summary>
    /// <param name="handleIfTarget"></param>
    public void SendToAllClients()
    {
        if (Util.IsServer)
        {
            // Server sends packets to all the clients
            var packet = GetPacket();
            Send(packet, SendType.SendToAllClients);
        }
        else if (Util.IsClient)
        {
            HandleFromTarget();

            // Send packet to server so it can forward it to everyone
            var packet = GetPacket();
            Send(packet, SendType.SendToAllClients);
        }
    }

    /// <summary>
    /// Sends this packet to all of the clients and the server
    /// </summary>
    /// <param name="handleIfTarget"></param>
    public void SendToAll()
    {
        if (Util.IsServer)
        {
            HandleFromTarget();

            // Server sends packets to all the clients
            var packet = GetPacket();
            Send(packet, SendType.SendToAll);
        }
        else if (Util.IsClient)
        {
            HandleFromTarget();

            // Send packet to server so it can forward it to everyone
            var packet = GetPacket();
            Send(packet, SendType.SendToAll);
        }
    }

    #endregion

    #region Handling

    internal void HandleSendToServer(int? fromWho)
    {
        if (Util.IsServer)
            Handle(fromWho);
    }

    internal void HandleSendToClient(BinaryReader reader, int? fromWho)
    {
        if (Util.IsServer)
        {
            // Forward the packet
            int toWho = reader.ReadInt32();
            var packet = GetPacket();
            Send(packet, SendType.SendToClient, toWho);
        }
        else if (Util.IsClient)
        {
            // Handle the packet
            Handle(fromWho);
        }
    }

    internal void HandleSendToClients(BinaryReader reader, int? fromWho)
    {
        if (Util.IsServer)
        {
            // Forward the packets
            int numClients = reader.ReadInt32();
            for (int i = 0; i < numClients; i++)
            {
                int toWho = reader.ReadInt32();
                if (Util.MachineID(toWho) == fromWho)
                    continue;

                // Sending new packet
                var packet = GetPacket();
                Send(packet, SendType.SendToClients, toWho);
            }
        }
        else if (Util.IsClient)
        {
            // Handle the packet
            Handle(fromWho);
        }
    }

    internal void HandleSendToAllClients(int? fromWho)
    {
        if (Util.IsServer)
        {
            // Send a packet to everyone
            var packet = GetPacket();
            Send(packet, SendType.SendToAllClients, ignoreWho: fromWho ?? -1);
        }
        else if (Util.IsClient)
        {
            // Handle the packet
            Handle(fromWho);
        }
    }

    internal void HandleSendToAll(int? fromWho)
    {
        if (Util.IsServer)
        {
            // Handle the packet
            Handle(fromWho);

            // Send a packet to everyone
            var packet = GetPacket();
            Send(packet, SendType.SendToAllClients, ignoreWho: fromWho ?? -1);
        }
        else if (Util.IsClient)
        {
            // Handle the packet
            Handle(fromWho);
        }
    }

    // The handle function called if the machine is the target of a send method
    internal void HandleFromTarget()
    {
        if (HandleIfTarget)
            Handle(Util.MachineID(Main.myPlayer));
    }

    #endregion
}
