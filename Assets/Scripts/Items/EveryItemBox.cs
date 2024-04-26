using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class EveryItemBox : MonoBehaviour, IFirstLoadCallback
{

    public void OnFirstLoad()
    {
        var inventory = GetComponent<Inventory>();

        foreach (var item in Items.GetAll())
        {
            for (int i = 0; i < 3; i++)
            {
                inventory.TryAdd(new ItemStack(item, item.StackSize));
            }
        }
    }

}
