using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{

    [SerializeField] private Weapon _debugWeaponToGrab;

    public Weapon ActiveWeapon { get; private set; }

    private void Start()
    {
        
    }

    public void Equip(Weapon weapon)
    {
        weapon.transform.SetParent(transform, false);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        ActiveWeapon = weapon;
    }

}
