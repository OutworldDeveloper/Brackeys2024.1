using UnityEngine;

public sealed class UI_LoadGameWindow : UI_Panel
{

    [SerializeField] private UI_SaveSlots _saveSlots;
    [SerializeField] private Prefab<UI_YesNoWindow> _yesNoWindow;

    private void Awake()
    {
        _saveSlots.SlotSelected += ClickedOnSlot;
    }

    private void OnEnable()
    {
        for (int i = 0; i < 6; i++)
        {
            if (SaveLoadSystem.HasDataInSlot(i) == true)
            {
                var data = SaveLoadSystem.GetSaveDataInSlot(i);
                _saveSlots.SetSlotInfo(i, new UI_SaveSlotInfo(data));
            }
            else
            {
                _saveSlots.ClearSlot(i);
            }
        }
    }

    private void ClickedOnSlot(int slot)
    {
        if (SaveLoadSystem.HasDataInSlot(slot) == true)
        {
            SaveLoadSystem.LoadDataFromSlot(slot);
        }
    }

}
