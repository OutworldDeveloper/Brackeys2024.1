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

    private ItemSlot _weaponSlot;

    private void Awake()
    {
        _weaponSlot = _character.GetComponent<Equipment>().WeaponSlot;
        _weaponSlot.Changed += OnWeaponSlotChanged;
        _weaponSlot.AttributesChanged += OnWeaponSlotAttributesChanged;
        _character.Inventory.Changed += OnInventoryChanged;
    }

    private void Start()
    {
        Refresh();
    }

    private void OnWeaponSlotChanged(ItemSlot slot)
    {
        Refresh();
    }

    private void OnWeaponSlotAttributesChanged(ItemSlot obj)
    {
        Refresh();
    }

    private void OnInventoryChanged()
    {
        Refresh();
    }

    private void Refresh()
    {
        _counterParent.gameObject.SetActive(_weaponSlot.IsEmpty == false);

        if (_weaponSlot.IsEmpty == true)
            return;

        WeaponItem weapon = _weaponSlot.Stack.Item as WeaponItem;

        int loadedCount = _weaponSlot.Stack.Attributes.Get(WeaponItem.LOADED_AMMO);
        int inventoryCount = _character.Inventory.GetAmountOf(weapon.AmmoItem);
        _ammoLabel.text = $"{loadedCount} / <size=30>{inventoryCount}</size>";
    }

}
