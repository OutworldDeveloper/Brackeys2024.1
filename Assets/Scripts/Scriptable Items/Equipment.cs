using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour, ICustomSaveable
{

    private ItemSlot _weaponSlot;

    public ItemSlot WeaponSlot => _weaponSlot;

    public void Initialize()
    {
        _weaponSlot = new ItemSlot(this, nameof(_weaponSlot), typeof(WeaponItem));
    }

    public object SaveData()
    {
        var data = new Dictionary<string, ItemStack>();

        Debug.Log(_weaponSlot.IsEmpty);

        if (_weaponSlot.IsEmpty == false)
            data.Add("weapon", _weaponSlot.GetStack());

        return data;
    }

    public void LoadData(object data)
    {
        var stacks = (Dictionary<string, ItemStack>)data;

        if (stacks.TryGetValue("weapon", out ItemStack stack) == true)
            _weaponSlot.TryAdd(stack);
    }

}
