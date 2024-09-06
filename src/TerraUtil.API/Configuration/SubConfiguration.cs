using Newtonsoft.Json;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace TerraUtil.Configuration;

/// <summary>
/// A type that can be extended from to make a sub-config.<br />
/// Automatically implements <see cref="object.Equals(object?)" /> and <see cref="object.GetHashCode" />.
/// </summary>

// TODO: broken? coud make record
public abstract class SubConfiguration
{
    public override bool Equals(object obj)
    {
        // If they are the same type then check if their fields are equal
        if (obj.GetType() == GetType())
        {
            // Check if the fields aren't equal
            foreach (var field in GetFields())
            {
                if (field.GetValue(this) != field.GetValue(obj))
                    return false;
            }

            // Otherwise return true
            return true;
        }

        // Otherwise normal check
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        var values = new List<object>();
        foreach (var field in GetFields())
        {
            values.Add(field.GetValue(this));
        }

        return values.GetHashCode();
    }

    private IEnumerable<PropertyFieldWrapper> GetFields()
    {
        return from field in ConfigManager.GetFieldsAndProperties(this)
               where ConfigManager.GetCustomAttributeFromMemberThenMemberType<JsonIgnoreAttribute>(field, this, null) == null
               select field;
    }
}
