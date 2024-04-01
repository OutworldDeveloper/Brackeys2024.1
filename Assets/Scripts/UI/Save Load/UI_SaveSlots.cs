using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SaveSlots : MonoBehaviour
{

    public event Action<int> SlotSelected;

    [SerializeField] private UI_SaveSlot[] _slots;

    private void Start()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            int x = i;
            _slots[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                SlotSelected?.Invoke(x);
            });
        }

        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (SaveLoadSystem.HasDataInSlot(i) == true)
            {
                var data = SaveLoadSystem.GetSaveDataInSlot(i);
                _slots[i].Display(new UI_SaveSlotInfo(data));
            }
            else
            {
                _slots[i].DisplayEmpty();
            }
        }
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
