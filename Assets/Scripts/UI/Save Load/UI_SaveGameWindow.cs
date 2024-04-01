using UnityEngine;

public sealed class UI_SaveGameWindow : UI_Panel
{

    [SerializeField] private UI_SaveSlots _saveSlots;
    [SerializeField] private Prefab<UI_YesNoWindow> _yesNoWindow;
    
    private void Awake()
    {
        _saveSlots.SlotSelected += ClickedOnSlot;
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

        _saveSlots.Refresh();

        Owner.InstantiateAndOpenFrom(_yesNoWindow).
            Setup("Success", "The game was successfuly saved.", () => { }, false);
    }

}
