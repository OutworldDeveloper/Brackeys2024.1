using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    public event Action<Item> ItemAdded;
    public event Action<Item> ItemRemoved;

    private readonly List<Item> _items = new List<Item>();

    public Item[] Items => _items.ToArray();

    public bool HasItemWithTag(ItemTag tag)
    {
        return TryGetItemWithTag(tag, out Item result);
    }

    public bool TryGetItemWithTag(ItemTag tag, out Item result)
    {
        foreach (var item in _items)
        {
            if (item.HasTag(tag) == true)
            {
                result = item;
                return true;
            }
        }

        result = null;
        return false;
    }

    public void AddItem(Item item)
    {
        item.gameObject.SetActive(false);
        _items.Add(item);
        ItemAdded?.Invoke(item);
    }

    public void RemoveItem(Item item)
    {
        item.gameObject.SetActive(true);
        _items.Remove(item);
        ItemRemoved?.Invoke(item);
    }

    public void RemoveAndDestroyItem(Item item)
    {
        RemoveItem(item);
        item.DestroyItem();
    }

}
