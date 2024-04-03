using UnityEngine;

public class Equipment : MonoBehaviour
{

    private ExpItemSlot _weaponSlot;

    public ExpItemSlot WeaponSlot => _weaponSlot;

    private void Awake()
    {
        _weaponSlot = new ExpItemSlot(this, nameof(_weaponSlot));
    }

}
