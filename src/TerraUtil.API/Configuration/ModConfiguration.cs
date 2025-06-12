using Newtonsoft.Json;
using Terraria.ModLoader.Config;

namespace TerraUtil.API.Configuration;

/// <summary>
/// A variant of <see cref="ModConfig" /> that automatically implements recursive <see cref="ModConfig.NeedsReload(ModConfig)" />.
/// </summary>

// TODO: broken?
public abstract class ModConfiguration : ModConfig
{
    private const int MaxDepth = 10;

    // Normally only top level fields trigger reloading with a mod config, which I have to fix myself
    public override bool NeedsReload(ModConfig pendingConfig)
    {
        return ObjectNeedsReload(this, pendingConfig, MaxDepth);
    }

    // Recursive function that checks if an object needs a reload, with a max depth
    private static bool ObjectNeedsReload(object currentConfig, object pendingConfig, int depth)
    {
        // Recursive limit check
        if (depth <= 0)
            return false;

        // Loop over every field to check if they have been changed
        foreach (var field in ConfigManager.GetFieldsAndProperties(currentConfig))
        {
            // If it has a reload required attribute and the field values don't match, then return true
            bool doesntHaveJsonIgnore = ConfigManager.GetCustomAttributeFromMemberThenMemberType<JsonIgnoreAttribute>(field, currentConfig, null) == null;
            bool hasReloadRequired = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ReloadRequiredAttribute>(field, currentConfig, null) != null;
            bool dontEqual = !ConfigManager.ObjectEquals(field.GetValue(currentConfig), field.GetValue(pendingConfig));
            if (doesntHaveJsonIgnore && hasReloadRequired && dontEqual)
                return true;

            // Otherwise if it's a sub config, then check that as well
            if (field.Type.IsSubclassOf(typeof(SubConfiguration)) && ObjectNeedsReload(field.GetValue(currentConfig), field.GetValue(pendingConfig), depth - 1))
                return true;
        }

        return false;
    }
}
