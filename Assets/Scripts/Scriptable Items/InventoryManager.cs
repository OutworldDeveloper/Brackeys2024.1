
// If it's on player, then we can check target inventory and stuff
// If slot.parent == player.Equipment
// If slot.parent == player.Inventory
// then good
using System;
using UnityEngine;

public static class InventoryManager
{

    public static bool TryTransfer(ItemSlot slotA, ItemSlot slotB, int amount)
    {
        if (amount <= 0)
            throw new Exception("Attempting to transfer an invalid amount of items.");

        if (slotA == slotB)
            throw new Exception("Attempting to transfer an item to the same slot it's already in.");

        if (slotA.IsEmpty == true)
            throw new Exception("Attempting to transfer an item from an empty slot.");

        if (slotB.IsCompatableWith(slotA.Stack.Item) == false)
            return false;

        if (slotA.Stack.Count < amount)
            return false;

        ItemStack stack = slotA.Take(amount);

        if (slotB.TryAdd(stack) == false)
        {
            slotA.TryAdd(stack); // Возвращаем с позором :D Может TakeCopy() ?
            return false;
        }

        return true;
    }

    public static bool TryTransfer(ItemSlot fromSlot, Inventory targetInventory, int amount)
    {
        if (amount <= 0)
            throw new Exception("Attempting to transfer an invalid amount of items.");

        if (fromSlot.Owner == targetInventory)
            throw new Exception("Attempting to transfer an item to the same inventory.");

        if (fromSlot.IsEmpty == true)
            throw new Exception("Attempting to transfer an item from an empty slot.");

        if (fromSlot.Stack.Count < amount)
            return false;

        ItemStack stack = fromSlot.Take(amount);

        if (targetInventory.TryAdd(stack) == false)
        {
            fromSlot.TryAdd(stack); // Возвращаем с позором :D Может TakeCopy() ?
            return false;
        }

        return true;
    }

    public static bool TryDestroyStack(ItemSlot slot, int amount)
    {
        if (amount <= 0)
            throw new Exception("Trying to destroy an invalid amount of items.");

        if (slot.IsEmpty == true)
            throw new Exception("Trying to destroy a stack in an empty slot.");

        slot.Take(amount);
        return true;
    }

    public static void TryDestroy(Inventory inventory, Item targetItem, int amount)
    {
        if (amount <= 0)
            throw new Exception("Trying to destroy an invalid amount of items.");

        for (int i = 0; i < inventory.SlotsCount; i++)
        {
            var slot = inventory[i];

            if (slot.IsEmpty == true)
                continue;

            if (slot.Stack.Item != targetItem)
                continue;

            ItemStack removedStack = slot.Take(Mathf.Min(slot.Stack.Count, amount));

            amount -= removedStack.Count;

            if (amount == 0)
                return;
        }
    }

}
