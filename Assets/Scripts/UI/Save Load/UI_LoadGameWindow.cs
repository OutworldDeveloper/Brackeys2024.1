using UnityEngine;

public sealed class UI_LoadGameWindow : UI_Panel
{

    [SerializeField] private UI_SaveSlots _saveSlots;
    [SerializeField] private Prefab<UI_YesNoWindow> _yesNoWindow;

    private void Awake()
    {
        _saveSlots.SlotSelected += ClickedOnSlot;
    }

    private void ClickedOnSlot(int slot)
    {
        if (SaveLoadSystem.HasDataInSlot(slot) == true)
        {
            SaveLoadSystem.LoadDataFromSlot(slot);
        }
    }

}
