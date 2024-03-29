using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SaveSlots : MonoBehaviour
{

    public event Action<int> SlotSelected;

    [SerializeField] private UI_SaveSlot[] _slots;

    private void Awake()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            int x = i;
            _slots[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                SlotSelected?.Invoke(x);
            });
        }
    }

    public void SetSlotInfo(int slot, UI_SaveSlotInfo info)
    {
        _slots[slot].Display(info);
    }

    public void ClearSlot(int slot)
    {
        _slots[slot].DisplayEmpty();
    }

}

public readonly struct UI_SaveSlotInfo
{
    public readonly DateTime LastSavedTime;
    public readonly float PlayTime;
    public readonly int SavedTimes;

    public UI_SaveSlotInfo(SaveData data)
    {
        LastSavedTime = data.LastSaveTime;
        PlayTime = 0f;
        SavedTimes = data.SavedTimes;
    }

}
