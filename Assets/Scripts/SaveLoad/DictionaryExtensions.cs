using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key) == true)
        {
            dictionary[key] = value;
            return;
        }

        dictionary.Add(key, value);
    }

}
