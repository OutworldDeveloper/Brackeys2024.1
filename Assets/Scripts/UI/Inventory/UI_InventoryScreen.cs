using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public class UI_InventoryScreen : UI_Panel
{

    [SerializeField] private UI_Slot _slotPrefab;
    [SerializeField] private Transform _slotsParent;

    [SerializeField] private ItemDefinition _itemTest;

    private ExpInventory _inventory;

    public void SetTarget(ExpInventory inventory)
    {
        _inventory = inventory;
    }

    private void Start()
    {
        for (int i = 0; i < _inventory.SlotsCount; i++)
        {
            var slot = _inventory[i];
            var slotUI = Instantiate(_slotPrefab, _slotsParent, false);
            slotUI.SetTarget(slot);
        }
    }

    public override void InputUpdate()
    {
        return;
        if (Input.GetKeyDown(KeyCode.Tab) == true)
        {
            CloseAndDestroy();
        }
    }

    private void Update()
    {
        KeyCode[] testKeys = new KeyCode[]
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.Alpha0,
        };

        for (int i = 0; i < testKeys.Length; i++)
        {
            if (Input.GetKeyDown(testKeys[i]) == true)
            {
                _inventory[i].TryAddStack(new ItemStack(_itemTest));
            }
        }
    }

}
