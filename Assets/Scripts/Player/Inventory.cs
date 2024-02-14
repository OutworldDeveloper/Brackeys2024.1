using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    public event Action<Item> ItemAdded;
    public event Action<Item> ItemRemoved;

    private readonly List<Item> _items = new List<Item>();

    public Item[] Content => _items.ToArray();

    public bool HasItemWithTag(ItemTag tag)
    {
        foreach (var item in _items)
        {
            if (item.HasTag(tag) == true)
                return true;
        }

        return false;
    }

    public bool HasItem(Item item)
    {
        foreach (var checkItem in _items)
        {
            if (checkItem == item)
                return true;
        }

        return false;
    }

    public void AddItem(Item item)
    {
        _items.Add(item);
        ItemAdded?.Invoke(item);
    }

    public void RemoveItem(Item item)
    {
        _items.Remove(item);
        ItemRemoved?.Invoke(item);
    }

}
