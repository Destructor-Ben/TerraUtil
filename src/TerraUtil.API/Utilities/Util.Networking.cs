namespace TerraUtil.Utilities;

public static partial class Util
{
    /// <summary>
    /// If the current game session is in singleplayer.
    /// </summary>
    public static bool IsSingleplayer => Main.netMode == NetmodeID.SinglePlayer;

    /// <summary>
    /// If the current game session is a client or server.
    /// </summary>
    public static bool IsMultiplayer => IsClient || IsServer;

    /// <summary>
    /// If the local machine is a dedicated server.
    /// </summary>
    public static bool IsHeadless => Main.dedServ;

    /// <summary>
    /// If the local machine is a multiplayer client.
    /// </summary>
    public static bool IsClient => Main.netMode == NetmodeID.MultiplayerClient;

    /// <summary>
    /// If the local machine is a server.
    /// </summary>
    public static bool IsServer => Main.netMode == NetmodeID.Server;

    /// <summary>
    /// Gets if the given ID is the server.
    /// </summary>
    /// <param name="whoAmI">The ID to check.</param>
    /// <returns><see langword="true" /> if <paramref name="whoAmI" /> is 255 or -1, else <see langword="false" />.</returns>
    public static bool IsServerID(int whoAmI)
    {
        return whoAmI is 255 or -1;
    }

    /// <summary>
    /// Gets the net ID of a machine.
    /// </summary>
    /// <param name="whoAmI">The ID of the machine.</param>
    /// <returns><see langword="null" /> if <paramref name="whoAmI" /> is the server, otherwise the whoAmI of a client.</returns>
    public static int? MachineID(int whoAmI)
    {
        return IsServerID(whoAmI) ? null : whoAmI;
    }
}
