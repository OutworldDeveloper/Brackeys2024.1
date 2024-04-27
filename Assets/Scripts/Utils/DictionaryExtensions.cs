using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static void Update<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key) == true)
        {
            dictionary[key] = value;
            return;
        }

        dictionary.Add(key, value);
    }

    public static TValue Resolve<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue fallback)
    {
        if (key == null)
            return fallback;

        if (dictionary.ContainsKey(key) == false)
            return fallback;

        return dictionary[key];
    }

}
