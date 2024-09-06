namespace TerraUtil.Utilities;

public static partial class Util
{
    /// <summary>
    /// Gets a <see cref="LocalizedText" /> from the given key.
    /// </summary>
    /// <param name="key">The key to the <see cref="LocalizedText" /> without the <c>Mods.{ModName}</c> prefix.</param>
    /// <returns>A <see cref="LocalizedText" /> with the given key with <c>Mods.{ModName}.</c> prepended to it.</returns>
    public static LocalizedText GetText(string key)
    {
        return Language.GetOrRegister($"Mods.{Mod.Name}.{key}");
    }

    /// <summary>
    /// Gets a <see cref="LocalizedText" /> value from the given key.
    /// </summary>
    /// <param name="key">The key to the <see cref="LocalizedText" /> without the <c>Mods.{ModName}</c> prefix.</param>
    /// <returns>A <see cref="LocalizedText" /> value with the given key with <c>Mods.{ModName}.</c> prepended to it.</returns>
    public static string GetTextValue(string key)
    {
        return GetText(key).Value;
    }

    /// <summary>
    /// Gets a <see cref="LocalizedText" /> value from the given key with the given format arguments.
    /// </summary>
    /// <param name="key">The key to the <see cref="LocalizedText" /> without the <c>Mods.{ModName}</c> prefix.</param>
    /// <param name="stringFormat">The objects to format the <see cref="LocalizedText" /> with.</param>
    /// <returns>A <see cref="LocalizedText" /> value with the given key with <c>Mods.{ModName}.</c> prepended to it and formatted with <paramref name="stringFormat" />.</returns>
    public static string GetTextValue(string key, params object[] stringFormat)
    {
        try
        {
            return GetText(key).Format(stringFormat);
        }
        catch (FormatException)
        {
            Mod.Logger.Warn($"Localization key \"{key}\" had invalid pluralization, make sure to use \"{{^0\"}}, not \"{{0^\"}}");
            return key;
        }
    }
}
