using UnityEngine;

public class Equipment : MonoBehaviour
{

    private ItemSlot _weaponSlot;

    public ItemSlot WeaponSlot => _weaponSlot;

    private void Awake()
    {
        _weaponSlot = new ItemSlot(this, nameof(_weaponSlot), typeof(WeaponItemDefinition));
    }

}
