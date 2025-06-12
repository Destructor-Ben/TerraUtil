using TerraUtil.API.Utilities;

namespace TerraUtil.API.RecipeGroups;

public class RecipeGroupSystem : TerraUtilLoader<ModRecipeGroup>
{
    public override void AddRecipeGroups()
    {
        foreach (var modGroup in Content)
        {
            var group = new RecipeGroup(() => Util.GetTextValue($"RecipeGroups.{modGroup.Name}"), modGroup.ValidItems.ToArray()) { IconicItemId = modGroup.ItemIconID };

            RecipeGroup.RegisterGroup(Mod.Name + ":" + modGroup.Name, group);
            modGroup.Group = group;
        }
    }
}
