using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_InventoryGrid : MonoBehaviour
{

    public event Action<UI_Slot> SlotSelected;
    public event Action<UI_Slot> SlotHovered;

    [SerializeField] private UI_Slot _slotPrefab;

    private ExpInventory _inventory;
    private readonly List<UI_Slot> _slots = new List<UI_Slot>();

    public void SetTarget(ExpInventory inventory)
    {
        _inventory = inventory;
    }

    public UI_Slot[] GetSlots()
    {
        return _slots.ToArray();
    }

    private void Start()
    {
        for (int i = 0; i < _inventory.SlotsCount; i++)
        {
            UI_Slot slotUI = Instantiate(_slotPrefab, transform, false);
            slotUI.Selected += OnSlotSelected;
            slotUI.Hovered += OnSlotHovered;
            slotUI.SetTarget(_inventory[i]);
            _slots.Add(slotUI);
        }
    }

    private void OnSlotHovered(UI_Slot slotUI)
    {
        SlotHovered?.Invoke(slotUI);
    }

    private void OnSlotSelected(UI_Slot slotUI)
    {
        SlotSelected?.Invoke(slotUI);
    }

}
