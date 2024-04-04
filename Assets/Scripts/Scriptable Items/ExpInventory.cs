using UnityEngine;

public class ExpInventory : MonoBehaviour
{

    [SerializeField] private int _slotsCount;

    private ItemSlot[] _slots;

    public int SlotsCount => _slots.Length;
    public ItemSlot this[int index] => _slots[index];

    private void Awake()
    {
        _slots = new ItemSlot[_slotsCount];

        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i] = new ItemSlot(this, $"Inventory{i}");
        }
    }

    public bool TryAdd(ItemStack stack)
    {
        foreach (var slot in _slots)
        {
            if (slot.TryAdd(stack) == true)
                return true;
        }
    
        return false;
    }

}
