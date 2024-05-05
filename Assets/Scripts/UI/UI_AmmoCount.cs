using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_AmmoCount : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;
    [SerializeField] private GameObject _counterParent;
    [SerializeField] private TextMeshProUGUI _ammoLabel;

    private ItemSlot _targetSlot;

    private void Awake()
    {
        _character.Equipment.ActiveSlotChanged += OnActiveSlotChanged;
        _character.Inventory.Changed += OnInventoryChanged;
    }

    private void Start()
    {
        OnActiveSlotChanged(-1, _character.Equipment.ActiveSlotIndex);
    }

    private void OnActiveSlotChanged(int previousIndex, int index)
    {
        if (_targetSlot != null)
            _targetSlot.Changed -= OnActiveSlotChanged;

        _targetSlot = _character.Equipment.ActiveSlot;
        _targetSlot.Changed += OnActiveSlotChanged;
        Refresh();
    }

    private void OnActiveSlotChanged(ItemSlot slot)
    {
        Refresh();
    }

    private void OnInventoryChanged()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (_targetSlot == null) // Preventing crashes
            return;

        _counterParent.gameObject.SetActive(_targetSlot.IsEmpty == false);

        if (_targetSlot.IsEmpty == true)
            return;

        if (_targetSlot.Stack.Item is WeaponItem == false)
            return;

        WeaponItem weapon = _targetSlot.Stack.Item as WeaponItem;

        int loadedCount = _targetSlot.Stack.Attributes.Get(WeaponItem.LOADED_AMMO);
        int inventoryCount = _character.Inventory.GetAmountOf(weapon.AmmoItem);
        _ammoLabel.text = $"{loadedCount} / <size=30>{inventoryCount}</size>";
    }

}
