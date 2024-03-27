using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSaveableComponentCustomData : MonoBehaviour, ICustomSaveable
{

    [SerializeField] private int _itemsAmount;

    public object SaveData()
    {
        return new InventoryData()
        {
            ItemsAmount = _itemsAmount
        };
    }

    public void LoadData(object data)
    {
        _itemsAmount = (data as InventoryData).ItemsAmount;
    }

    private class InventoryData
    {
        public int ItemsAmount;
    }

}
