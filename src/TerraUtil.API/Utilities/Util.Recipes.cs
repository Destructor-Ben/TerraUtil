using TerraUtil.RecipeGroups;

namespace TerraUtil.Utilities;

public static partial class Util
{
    /// <summary>
    /// Removes all recipes from <paramref name="type" /> that aren't created by this mod.
    /// </summary>
    /// <param name="type">The item type to remove the recipes from.</param>
    public static void RemoveRecipesForItem(int type)
    {
        foreach (var recipe in Main.recipe)
        {
            if (recipe.Mod != Mod && recipe.createItem.type == type)
                recipe.DisableRecipe();
        }
    }

    /// <summary>
    /// Adds a recipe group ingredient to this recipe with the given RecipeGroup name and stack size.
    /// </summary>
    /// <typeparam name="T">The <see cref="ModRecipeGroup" />.</typeparam>
    /// <param name="recipe">The recipe to add the group to.</param>
    /// <param name="stack">The amount of the recipe group to add.</param>
    /// <returns>The given recipe to allow for chaining.</returns>
    public static Recipe AddRecipeGroup<T>(this Recipe recipe, int stack = 1) where T : ModRecipeGroup
    {
        recipe.AddRecipeGroup(ModContent.GetInstance<T>().Group, stack);
        return recipe;
    }
}
