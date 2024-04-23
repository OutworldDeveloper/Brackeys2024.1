using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour, ICustomSaveable
{
    public event Action Changed;

    [SerializeField] private int _slotsCount;

    private ItemSlot[] _slots;

    public int SlotsCount => _slots.Length;
    public ItemSlot this[int index] => _slots[index];

    private void Awake()
    {
        _slots = new ItemSlot[_slotsCount];

        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i] = new ItemSlot(this, $"Inventory{i}");
            _slots[i].Changed += OnSlotChanged;
        }
    }

    private void OnSlotChanged(ItemSlot slot)
    {
        Changed?.Invoke();
    }

    public bool CanAdd(ItemStack stack)
    {
        foreach (var slot in _slots)
        {
            if (slot.CanAdd(stack) == true)
                return true;
        }

        return false;
    }

    public bool TryAdd(ItemStack stack)
    {
        foreach (var slot in _slots)
        {
            if (slot.TryAdd(stack) == true)
                return true;
        }
    
        return false;
    }

    public int GetAmountOf(Item item)
    {
        int count = 0;

        foreach (var slot in _slots)
        {
            if (slot.IsEmpty == true)
                continue;

            if (slot.Stack.Item == item)
                count += slot.Stack.Count;
        }

        return count;
    }

    public object SaveData()
    {
        var stacks = new Dictionary<int, ItemStack>();

        for (int i = 0; i < _slots.Length; i++)
        {
            var slot = _slots[i];

            if (slot.IsEmpty == false)
                stacks.Add(i, slot.GetStack());
        }

        return stacks;
    }

    public void LoadData(object data)
    {
        var stacks = (Dictionary<int, ItemStack>)data;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (stacks.ContainsKey(i) == false)
                continue;

            _slots[i].TryAdd(stacks[i]);
        }
    }

}
