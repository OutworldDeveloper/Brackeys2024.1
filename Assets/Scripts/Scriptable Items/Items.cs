using System.Collections.Generic;
using UnityEngine;

public static class Items
{

    private static Dictionary<string, Item> _items = new Dictionary<string, Item>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadItems()
    {
        foreach (var itemDefinition in Resources.LoadAll<Item>(string.Empty))
        {
            _items.Add(itemDefinition.name, itemDefinition);
        }
    }

    public static Item Get(string id)
    {
        return _items[id];
    }

}
