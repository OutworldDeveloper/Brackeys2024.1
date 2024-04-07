using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public sealed class UI_InventoryAndContainerScreen : UI_InventoryScreen
{

    [SerializeField] private UI_InventoryGrid _containerGrid;

    private Inventory _targetContainer;

    protected override bool ShowDestroyOption => false;

    public void SetContainer(Inventory inventory)
    {
        _targetContainer = inventory;
        _containerGrid.SetTarget(inventory);
    }

    protected override void Start()
    {
        base.Start();
        RegisterGrid(_containerGrid);
    }

    protected override void CreateActionsFor(UI_Slot slot, List<ItemAction> actions)
    {
        base.CreateActionsFor(slot, actions);

        if (slot.TargetSlot.IsEmpty == true)
            return;

        if (slot.TargetSlot.Owner != _targetContainer)
        {
            actions.Add(new ItemAction("Place inside", 
                () => InventoryManager.TryTransfer(slot.TargetSlot, _targetContainer, slot.TargetSlot.Stack.Count)));
        }
        else
        {
            actions.Add(new ItemAction("Take out", 
                () => InventoryManager.TryTransfer(slot.TargetSlot, Character.Inventory, slot.TargetSlot.Stack.Count)));
        }
    }

}
