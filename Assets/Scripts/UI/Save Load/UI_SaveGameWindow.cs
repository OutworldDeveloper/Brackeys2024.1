using UnityEngine;

public sealed class UI_SaveGameWindow : UI_Panel
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
        if (SaveLoadSystem.HasDataInSlot(slot) == false)
        {
            TrySaveToSlot(slot);
            return;
        }

       Owner.InstantiateAndOpenFrom(_yesNoWindow).
            Setup("Override slot?", "The data will be overriden.", () => TrySaveToSlot(slot));
    }

    private void TrySaveToSlot(int slot)
    {
        if (Input.GetKey(KeyCode.LeftControl)) // debug only
        {
            Debug.Log($"Loading {slot}");
            SaveLoadSystem.LoadDataFromSlot(slot);
            return;
        }

        SaveLoadSystem.SaveCurrentDataToSlot(slot);

        // Костыль (обновить слот)
        var data = SaveLoadSystem.GetSaveDataInSlot(slot);
        _saveSlots.SetSlotInfo(slot, new UI_SaveSlotInfo(data));

        Owner.InstantiateAndOpenFrom(_yesNoWindow).
            Setup("Success", "The game was successfuly saved.", () => { }, false);
    }

}
