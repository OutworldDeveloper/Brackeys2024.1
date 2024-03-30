using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveLoadSystem
{
    public static SaveData CurrentSave { get; private set; } = new SaveData();

    public static string GetSavePath(int slot) => $"{Application.persistentDataPath}/save{slot}.txt";

    public static SaveData GetSaveDataInSlot(int slot)
    {
        return BinarySaveDataSerializer.Deserialize(GetSavePath(slot));
    }

    public static void SaveCurrentDataToSlot(int slot)
    {
        SaveLoadScene.Current?.SaveData(); // Костыль?
        CurrentSave.LastSaveTime = DateTime.Now;
        CurrentSave.SavedTimes++;
        BinarySaveDataSerializer.Serialize(GetSavePath(slot), CurrentSave);
    }

    public static bool HasDataInSlot(int slot)
    {
        return BinarySaveDataSerializer.DoesFileExist(GetSavePath(slot));
    }

    public static void LoadDataFromSlot(int slot)
    {
        if (HasDataInSlot(slot) == false)
            Debug.LogError("Trying to load an empty slot");

        CurrentSave = GetSaveDataInSlot(slot);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // CurrentSave should have a scene name to load
    }

}
