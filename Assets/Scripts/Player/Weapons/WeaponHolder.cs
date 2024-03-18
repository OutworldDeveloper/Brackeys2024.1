using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{

    [SerializeField] private Weapon _debugWeaponToGrab;
    [SerializeField] private Transform _weaponBone;

    public Weapon ActiveWeapon { get; private set; }

    private void Start()
    {
        Equip(_debugWeaponToGrab);
    }

    public void Equip(Weapon weapon)
    {
        weapon.transform.SetParent(_weaponBone, false);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        ActiveWeapon = weapon;
    }

}
