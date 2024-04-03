using UnityEngine;

public class ExpInventory : MonoBehaviour
{

    [SerializeField] private int _slotsCount;

    private ExpItemSlot[] _slots;

    public int SlotsCount => _slots.Length;
    public ExpItemSlot this[int index] => _slots[index];

    private void Awake()
    {
        _slots = new ExpItemSlot[_slotsCount];

        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i] = new ExpItemSlot(this, $"Inventory [{i}]");
        }
    }

    public bool TryAdd(Item item)
    {
        foreach (var slot in _slots)
        {
            if (slot.TryAdd(item) == true)
                return true;
        }

        return false;
    }

}

// If it's on player, then we can check target inventory and stuff
// If slot.parent == player.Equipment
// If slot.parent == player.Inventory
// then good
public static class InventoryManager
{

    public static bool TryTransfer(ExpItemSlot from, ExpItemSlot to)
    {
        if (from == to)
            throw new System.Exception("Attempting to transfer an item to the same slot it's already in.");

        if (from.IsEmpty == true)
            throw new System.Exception("Attempting to transfer an item from an empty slot.");

        if (to.CanAdd(from.FirstItem) == true)
        {
            to.TryAdd(from.Take());
            return true;
        }

        return false;
    }

}
