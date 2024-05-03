using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(Order.UI)]
public sealed class UI_InventorySelectScreen : UI_InventoryScreen
{

    private ItemSelector _itemSelector;

    protected override bool ShowDestroyOption => false;

    public void SetSelector(ItemSelector itemSelector)
    {
        _itemSelector = itemSelector;
    }

    protected override void OnSlotSelected(UI_Slot slot)
    {
        TrySelect(slot);
    }

    protected override void CreateActionsFor(UI_Slot slot, List<ItemAction> actions)
    {
        if (slot.TargetSlot.IsEmpty == true)
            return;

        actions.Add(new ItemAction("Select", () => TrySelect(slot)));
    }

    private bool CanAccept(UI_Slot slot)
    {
        return _itemSelector.CanAccept(slot.TargetSlot.Stack);
    }

    private void TrySelect(UI_Slot slot)
    {
        if (slot.TargetSlot.IsEmpty == true)
            return;

        if (CanAccept(slot) == false)
        {
            Notification.Show(_itemSelector.GetRejectionReason(slot.TargetSlot.Stack), 1.25f);
            return;
        }

        _itemSelector.Select(slot.TargetSlot.GetStack());
        CloseAndDestroy();
    }

}

public abstract class ItemSelector
{

    public const string DEFAULT_REJECTION_TEXT = "Won't work";

    public abstract bool CanAccept(IReadOnlyStack stack);
    public abstract void Select(ItemStack stack);
    public virtual string GetRejectionReason(IReadOnlyStack stack) => DEFAULT_REJECTION_TEXT;

}
