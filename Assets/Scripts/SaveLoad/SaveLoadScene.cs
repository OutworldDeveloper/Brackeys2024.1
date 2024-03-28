using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.UnityConverters;
using Newtonsoft.Json.UnityConverters.Math;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

[DefaultExecutionOrder(-1000)]
public class SaveLoadScene : MonoBehaviour
{
    private static string SavePath => $"{Application.persistentDataPath}/save.txt";

    private readonly JsonSerializer _serializer;
    private JsonSerializerSettings _settings;

    private LevelData _sceneData;

    private BinaryFormatter _binaryFormatter;

    private void Awake()
    {
        _settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Include,

            TypeNameHandling = TypeNameHandling.Auto,

            Formatting = Formatting.Indented,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,

            ReferenceLoopHandling = ReferenceLoopHandling.Error, // Fixes vector3 and stuff

            DefaultValueHandling = DefaultValueHandling.Ignore,

            Converters = new JsonConverter[]
            {
                new Vector3Converter(),
                new QuaternionConverter(),
            },
        };

        _binaryFormatter = new BinaryFormatter();

        SurrogateSelector surrogateSelector = new SurrogateSelector();
        Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();

        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
        _binaryFormatter.SurrogateSelector = surrogateSelector;

        // Если false всё равно можно прогреть и удалить все обьекты что бы не было отличий никак
        if (File.Exists(SavePath) == false)
        {
            _sceneData = new LevelData();

            foreach (var dynamicSaveable in FindObjectsOfType<DynamicSaveable>())
            {
                _sceneData.DynamicTheyExist.Add(dynamicSaveable.SceneGuid);
            }

            foreach (var staticSaveable in FindObjectsOfType<StaticSaveable>())
            {
                var components = staticSaveable.GetComponent<SaveableComponents>().GetAll();

                foreach (var component in components)
                {
                    (component as IFreshStartable)?.FreshStart(); 
                    // This is wrong! We should do it no matter if we loaded or created save
                    // So items added in patches receive the callback too!
                }
            }

            return;
        }

        //var text = File.ReadAllText(SavePath);
        //_sceneData = JsonConvert.DeserializeObject<LevelData>(text, _settings);

        //using (StreamReader stream = File.OpenText(SavePath))
        //{
        //    _sceneData = (LevelData)_serializer.Deserialize(stream, typeof(LevelData));
        //}

        using (var stream = File.Open(SavePath, FileMode.Open))
        {
            _sceneData =  (LevelData)_binaryFormatter.Deserialize(stream);
        }

        foreach (var dynamicSaveable in FindObjectsOfType<DynamicSaveable>())
        {
            //if (_sceneData.ContainsDynamicSaveable(dynamicSaveable.SceneGuid) == true)
            //    Destroy(dynamicSaveable.gameObject);
            
            if(_sceneData.DynamicTheyExist.Contains(dynamicSaveable.SceneGuid) == true)
                Destroy(dynamicSaveable.gameObject);
        }

        // Load dynamic objects
        foreach (var dynamicSaveableData in _sceneData.DynamicSaveableDatas)
        {
            string resourcePath = dynamicSaveableData.ResourcesPath;
            //Debug.Log(resourcePath);
            resourcePath = PrefabPathToResourcePath(resourcePath);
            //Debug.Log(resourcePath);
            DynamicSaveable dynamicSaveable = Instantiate(Resources.Load<DynamicSaveable>(resourcePath));

            dynamicSaveable.SetSceneGuid(dynamicSaveableData.Guid);

            dynamicSaveable.transform.position = dynamicSaveableData.Position;
            dynamicSaveable.transform.eulerAngles = dynamicSaveableData.Rotation;

            dynamicSaveable.GetComponent<SaveableComponents>().RestoreState(dynamicSaveableData.Components);

            Debug.Log($"Spawned '{resourcePath}'");
        }

        // Load static objects
        foreach (var staticSaveable in FindObjectsOfType<StaticSaveable>())
        {
            if (_sceneData.StaticSaveableDatas.TryGetValue(staticSaveable.SceneGuid, out StaticSaveableData staticSaveableData) == false)
                continue;

            staticSaveable.transform.position = staticSaveableData.Position;
            staticSaveable.transform.eulerAngles = staticSaveableData.Rotation;

            staticSaveable.GetComponent<SaveableComponents>().RestoreState(staticSaveableData.Components);
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
        //var levelData = new LevelData(); // We used to create new LevelData every time. Let's try to keep the old one

        // we still need to clear it though
        _sceneData.DynamicSaveableDatas.Clear();
        _sceneData.StaticSaveableDatas.Clear();
        //

        foreach (var dynamicSaveable in FindObjectsOfType<DynamicSaveable>())
        {
            // Don't save those objects that have StaticSaveable as parents
            if (dynamicSaveable.GetComponentInParent<StaticSaveable>() != null)
                continue;

            _sceneData.DynamicSaveableDatas.Add(new DynamicSaveableData()
            {
                ResourcesPath = dynamicSaveable.PrefabPath,
                Guid = dynamicSaveable.SceneGuid,
                Position = dynamicSaveable.transform.position,
                Rotation = dynamicSaveable.transform.eulerAngles,
                //CustomData = dynamicSaveable.GatherData(),
                Components = dynamicSaveable.GetComponent<SaveableComponents>().GatherData()
            });
        }

        // Static
        foreach (var staticSaveable in FindObjectsOfType<StaticSaveable>())
        {
            _sceneData.StaticSaveableDatas.Add(staticSaveable.SceneGuid, new StaticSaveableData()
            {
                Position = staticSaveable.transform.position,
                Rotation = staticSaveable.transform.eulerAngles,
                Components = staticSaveable.GetComponent<SaveableComponents>().GatherData()
            });
        }

        // Save file
        //string text = JsonConvert.SerializeObject(_sceneData, _settings);
        //File.WriteAllText(SavePath, text);

        //using (var stream = File.CreateText(SavePath))
        //{
        //    _serializer.Serialize(stream, _sceneData);
        //}

        using (var stream = File.Open(SavePath, FileMode.Create))
        {
            _binaryFormatter.Serialize(stream, _sceneData);
        }
    }

}


[Serializable]
public class LevelData
{

    public Dictionary<string, StaticSaveableData> StaticSaveableDatas = new Dictionary<string, StaticSaveableData>();
    public List<DynamicSaveableData> DynamicSaveableDatas = new List<DynamicSaveableData>();

    public HashSet<string> DynamicTheyExist = new HashSet<string>(); // Dynamic objects that do exist or something

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
    public Vector3 Position;
    public Vector3 Rotation;
    public ComponentsData Components;

}

[Serializable]
public struct StaticSaveableData
{
    public Vector3 Position;
    public Vector3 Rotation;
    public ComponentsData Components;
    public bool IsStarted;

}

[Serializable]
public sealed class SaveData
{
    // For non scene static objects. They will be deserialized in every scene
    // Example: Player
    public Dictionary<string, StaticSaveableData> StaticSaveableDatas = new Dictionary<string, StaticSaveableData>();
    public Dictionary<string, LevelData> ScenesData = new Dictionary<string, LevelData>();

}

public static class DictionaryExtensions
{
    public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key) == true)
        {
            dictionary[key] = value;
            return;
        }

        dictionary.Add(key, value);
    }

}

public class Vector3SerializationSurrogate : ISerializationSurrogate
{

    // Method called to serialize a Vector3 object
    public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
    {
        Vector3 v3 = (Vector3)obj;
        info.AddValue("x", v3.x);
        info.AddValue("y", v3.y);
        info.AddValue("z", v3.z);
    }

    // Method called to deserialize a Vector3 object
    public System.Object SetObjectData(System.Object obj, SerializationInfo info,
                                       StreamingContext context, ISurrogateSelector selector)
    {
        Vector3 v3 = (Vector3)obj;
        v3.x = (float)info.GetValue("x", typeof(float));
        v3.y = (float)info.GetValue("y", typeof(float));
        v3.z = (float)info.GetValue("z", typeof(float));
        obj = v3;
        return obj;
    }

}