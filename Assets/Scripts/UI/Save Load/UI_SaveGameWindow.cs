using UnityEngine;

public sealed class UI_SaveGameWindow : MonoBehaviour
{

    [SerializeField] private UI_SaveSlots _saveSlots;
    
    private void Awake()
    {
        _saveSlots.SlotSelected += TrySaveToSlot;
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

    private void TrySaveToSlot(int slot)
    {
        if (Input.GetKey(KeyCode.LeftControl)) // debug only
        {
            Debug.Log($"Loading {slot}");
            SaveLoadSystem.LoadDataFromSlot(slot);
            return;
        }

        SaveLoadSystem.SaveCurrentDataToSlot(slot);

        // Костыль
        var data = SaveLoadSystem.GetSaveDataInSlot(slot);
        _saveSlots.SetSlotInfo(slot, new UI_SaveSlotInfo(data));
    }

}
