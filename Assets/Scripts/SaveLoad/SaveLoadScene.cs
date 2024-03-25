using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class SaveLoadScene : MonoBehaviour
{
    private static string SavePath => $"{Application.persistentDataPath}/save.txt";

    private readonly JsonSerializer _serializer = new JsonSerializer()
    {
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented,
        MissingMemberHandling = MissingMemberHandling.Error,
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
    };

    private void Awake()
    {
        bool hasData = File.Exists(SavePath);

        // Если false всё равно можно прогреть и удалить все обьекты что бы не было отличий никак
        if (hasData == false)
            return;

        var sceneData = new LevelData(); // loadedSave

        using (StreamReader stream = File.OpenText(SavePath))
        {
            sceneData = (LevelData)_serializer.Deserialize(stream, typeof(LevelData));
        }

        foreach (var dynamicSaveable in FindObjectsOfType<DynamicSaveable>())
        {
            if (sceneData.ContainsDynamicSaveable(dynamicSaveable.SceneGuid) == true)
                Destroy(dynamicSaveable.gameObject);
        }

        // Load objects
        foreach (var dynamicSaveableData in sceneData.DynamicSaveableDatas)
        {
            string resourcePath = dynamicSaveableData.ResourcesPath;
            //Debug.Log(resourcePath);
            resourcePath = PrefabPathToResourcePath(resourcePath);
            //Debug.Log(resourcePath);
            DynamicSaveable dynamicSaveable = Instantiate(Resources.Load<DynamicSaveable>(resourcePath));

            dynamicSaveable.SetSceneGuid(dynamicSaveableData.Guid);

            dynamicSaveable.transform.position = dynamicSaveableData.Position;
            dynamicSaveable.transform.eulerAngles = dynamicSaveableData.Rotation;

            dynamicSaveable.RestoreState(dynamicSaveableData.CustomData);

            Debug.Log($"Spawned '{resourcePath}'");
        }
    }

    private string PrefabPathToResourcePath(string path)
    {
        int index = path.IndexOf("Resources/");
        if (index >= 0)
        {
            string afterResources = path.Substring(index + "Resources/".Length);
            int prefabIndex = afterResources.LastIndexOf(".prefab");
            if (prefabIndex >= 0)
            {
                return afterResources.Substring(0, prefabIndex);
            }
            return afterResources;
        }
        return path;
    }

    [ContextMenu("Save")]
    public void SaveGame()
    {
        var levelData = new LevelData();

        foreach (var dynamicSaveable in FindObjectsOfType<DynamicSaveable>())
        {
            levelData.DynamicSaveableDatas.Add(new DynamicSaveableData()
            {
                ResourcesPath = dynamicSaveable.PrefabPath,
                Guid = dynamicSaveable.SceneGuid,
                Position = dynamicSaveable.transform.position,
                Rotation = dynamicSaveable.transform.eulerAngles,
                CustomData = dynamicSaveable.GatherData(),
            });
        }

        using (var stream = File.CreateText(SavePath))
        {
            _serializer.Serialize(stream, levelData);
        }
    }

}

[Serializable]
public class LevelData
{

    public List<DynamicSaveableData> DynamicSaveableDatas = new List<DynamicSaveableData>();

    public bool ContainsDynamicSaveable(string guid)
    {
        foreach (var dynamicSaveableData in DynamicSaveableDatas)
        {
            if (dynamicSaveableData.Guid == guid)
                return true;
        }

        return false;
    }

}

[Serializable]
public struct DynamicSaveableData
{
    public string Guid;
    public string ResourcesPath;
    public SerializableVector3 Position;
    public SerializableVector3 Rotation;
    public Dictionary<string, object> CustomData; // Not custom data but components data (custom data is different)

}