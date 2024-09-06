namespace TerraUtil.Utilities;

public static partial class Util
{
    /// <summary>
    /// Applies the equip effects of <paramref name="itemType" /> to <paramref name="player" />.
    /// </summary>
    /// <param name="player">The player to apply the effects to.</param>
    /// <param name="itemType">The item type that will have it's effects applied.</param>
    /// <param name="hideVisual">Whether visual effects will be applied.</param>
    public static void CopyVanillaEquipEffects(this Player player, int itemType, bool hideVisual = false)
    {
        var item = new Item();
        item.SetDefaults(itemType);
        player.ApplyEquipFunctional(item, hideVisual);
    }
}
