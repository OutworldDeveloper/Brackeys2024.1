using UnityEngine;

public class ExpInventory : MonoBehaviour
{

    [SerializeField] private int _slotsCount;

    private ExpItemSlot[] _slots;

    public int SlotsCount => _slots.Length;
    public ExpItemSlot this[int index] => _slots[index];

    private void Awake()
    {
        Debug.Log("Inventory");

        _slots = new ExpItemSlot[_slotsCount];

        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i] = new ExpItemSlot();
        }
    }

}
