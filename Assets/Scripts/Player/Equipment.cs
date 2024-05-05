using System;
using UnityEngine;

public class Equipment : Inventory
{

    public event Action<int, int> ActiveSlotChanged;

    [Persistent] private int _activeSlotIndex = 0;

    public int ActiveSlotIndex => _activeSlotIndex;
    public ItemSlot ActiveSlot => this[_activeSlotIndex];

    public void SetActiveSlot(int slot)
    {
        Debug.Assert(slot >= 0 && slot < SlotsCount, "Select");

        if (slot == _activeSlotIndex)
            return;

        int previousIndex = _activeSlotIndex;
        _activeSlotIndex = slot;
        ActiveSlotChanged?.Invoke(previousIndex, _activeSlotIndex);
    }

    protected override ItemSlot CreateSlot(int index)
    {
        return new ItemSlot(this, $"Hotbar{index}", typeof(WeaponItem));
    }

}
