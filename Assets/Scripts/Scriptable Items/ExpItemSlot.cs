using System;
using System.Collections.Generic;
using UnityEngine;

public class ExpItemSlot
{

    public event Action<ExpItemSlot> Changed;

    public readonly MonoBehaviour Owner;
    public readonly string SlotName;
    private readonly List<Item> _items = new List<Item>();

    public bool IsEmpty => _items.Count < 1;
    public Item FirstItem => _items[0];
    public int ItemsCount => _items.Count;

    public ExpItemSlot(MonoBehaviour owner, string name)
    {
        Owner = owner;
        SlotName = name;
    }

    public Item GetItem(int index)
    {
        return _items[index];
    }

    public Item[] GetItems()
    {
        return _items.ToArray();
    }

    public Item Take()
    {
        if (IsEmpty == true)
            throw new Exception("Trying to take an item from an Empty slot.");

        return RemoveAt(0);
    }

    // TODO: Upgrade
    public Item RemoveAt(int index)
    {
        Item item = _items[index];
        item.transform.SetParent(Owner.transform, false);
        item.DisableCollision();
        item.DisableVisuals();
        _items.RemoveAt(index);
        Changed?.Invoke(this);
        return item;
    }

    public bool TryAdd(Item item)
    {
        if (CanAdd(item) == false)
            return false;

        item.transform.SetParent(Owner.transform, false);
        item.DisableCollision();
        item.DisableVisuals();
        _items.Add(item);
        Changed?.Invoke(this);
        return true;
    }

    public bool CanAdd(Item item)
    {
        if (IsEmpty == true)
            return true;

        return FirstItem.StackType != null && FirstItem.StackType == item.StackType && ItemsCount + 1 <= FirstItem.StackType.MaxCount;
    }

    public override string ToString()
    {
        return $"Slot {SlotName} [" + (IsEmpty ? "Empty" : FirstItem.DisplayName) + "]"; 
    }

}
