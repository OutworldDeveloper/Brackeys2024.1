using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_AmmoCount : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;
    [SerializeField] private TextMeshProUGUI _ammoLabel;

    private void Awake()
    {
        _character.GetComponent<Equipment>().WeaponSlot.Changed += OnWeaponSlotChanged;
    }

    private void OnWeaponSlotChanged(ItemSlot slot)
    {
        _ammoLabel.gameObject.SetActive(slot.IsEmpty == false);

        if (slot.IsEmpty == true)
            return;

        int ammoCount = slot.Stack.Components.Get<LoadedAmmoComponent>().Value;
        int allCount = 15;
        _ammoLabel.text = $"{ammoCount} / <size=30>{allCount}</size>";
    }

}
