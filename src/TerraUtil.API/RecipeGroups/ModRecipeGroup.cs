namespace TerraUtil.RecipeGroups;

/// <summary>
/// An abstraction for <see cref="RecipeGroup" />.
/// </summary>
public abstract class ModRecipeGroup : TerraUtilModType
{
    /// <summary>
    /// The list of items this recipe groups contains.
    /// </summary>

    // TODO: change this to an ienumerable metho that uses yield return
    public abstract List<int> ValidItems { get; }

    /// <summary>
    /// The item ID of the icon of this recipe group.
    /// </summary>
    public virtual int ItemIconID => ValidItems[0];

    /// <summary>
    /// The instance of the <see cref="RecipeGroup" />.
    /// </summary>
    public RecipeGroup Group { get; internal set; }

    protected sealed override void Register()
    {
        ModTypeLookup<ModRecipeGroup>.Register(this);
    }

    public sealed override void SetupContent()
    {
        SetStaticDefaults();
    }
}
