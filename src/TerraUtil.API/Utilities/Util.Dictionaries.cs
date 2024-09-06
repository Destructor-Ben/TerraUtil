namespace TerraUtil.Utilities;

public static partial class Util
{
    /// <summary>
    /// Gets the <paramref name="key" /> in <paramref name="dict" /> if it exists, otherwise returns <paramref name="defaultVal" />.
    /// </summary>
    /// <typeparam name="T1">The key type of <paramref name="dict" />.</typeparam>
    /// <typeparam name="T2">The value type of <paramref name="dict" />.</typeparam>
    /// <param name="dict">The dictionary to get the value from.</param>
    /// <param name="key">The key to try and get the value with.</param>
    /// <param name="defaultVal">The value to return if <paramref name="key" /> isn't found.</param>
    /// <returns>The value in <paramref name="dict" /> from <paramref name="key" /> if it exists, otherwise <paramref name="defaultVal" />.</returns>
    public static T2 TryGetOrGiven<T1, T2>(this Dictionary<T1, T2> dict, T1 key, T2 defaultVal)
    {
        return dict.ContainsKey(key) ? dict[key] : defaultVal;
    }
}
