using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public sealed class UI_ItemActionButton : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _actionLabel;

    private UI_InventoryScreen _inventoryScreen;
    private ItemAction _action;

    public void SetAction(UI_InventoryScreen inventoryScreen, ItemAction action)
    {
        _inventoryScreen = inventoryScreen;
        _action = action;
        _actionLabel.text = action.DisplayName;
    }

    public void OnButtonPressed()
    {
        _inventoryScreen.TryExecuteItemAction(_action);
    }

}
