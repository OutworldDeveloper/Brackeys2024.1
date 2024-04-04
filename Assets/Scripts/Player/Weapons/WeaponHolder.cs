using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{

    [SerializeField] private Transform _weaponBone;

    public Weapon ActiveWeapon { get; private set; }
    public bool IsWeaponEquiped => ActiveWeapon != null;

    public void Equip(Prefab<Weapon> weapon)
    {
        RemoveWeapon();

        ActiveWeapon = weapon.Instantiate();
        ActiveWeapon.transform.SetParent(_weaponBone, false);
        ActiveWeapon.transform.localPosition = Vector3.zero;
        ActiveWeapon.transform.localRotation = Quaternion.identity;
    }

    public void RemoveWeapon()
    {
        if (ActiveWeapon != null)
            Destroy(ActiveWeapon.gameObject);
    }

}
