using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public sealed class UI_InventorySelectScreen : UI_InventoryScreen
{

    private IItemSelector _itemSelector;

    public void SetSelector(IItemSelector itemSelector)
    {
        _itemSelector = itemSelector;
    }

    protected override void OnSlotSelected(UI_Slot slot)
    {
        if (slot.TargetSlot.IsEmpty == true)
            return;

        if (_itemSelector.CanAccept(slot.TargetSlot.Stack) == true)
        {
            _itemSelector.Select(slot.TargetSlot.GetStack());
            CloseAndDestroy();
        }
    }

    protected override void OnSlotSelectedAlt(UI_Slot slot)
    {
        return;
    }

}

public interface IItemSelector
{
    public bool CanAccept(IReadOnlyStack stack);
    public void Select(ItemStack stack);

}
