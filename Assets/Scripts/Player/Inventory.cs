using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour, ICustomSaveable
{

    public event Action<Item> ItemAdded;
    public event Action<Item> ItemRemoved;

    private readonly List<Item> _items = new List<Item>();

    public Item[] Items => _items.ToArray();

    public bool HasItemWithTag(ItemTag tag)
    {
        return TryGetItemWithTag(tag, out Item result);
    }

    public bool TryGetItemWithTag(ItemTag tag, out Item result)
    {
        foreach (var item in _items)
        {
            if (item.HasTag(tag) == true)
            {
                result = item;
                return true;
            }
        }

        result = null;
        return false;
    }

    public void AddItem(Item item)
    {
        item.transform.SetParent(gameObject.transform, true);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.DisableCollision();
        item.DisableVisuals();
        _items.Add(item);
        ItemAdded?.Invoke(item);
    }

    public void RemoveItem(Item item)
    {
        item.transform.SetParent(null, true);
        item.EnableCollision();
        item.EnableVisuals();
        _items.Remove(item);
        ItemRemoved?.Invoke(item);
    }

    public void RemoveAndDestroyItem(Item item)
    {
        RemoveItem(item);
        item.Kill();
    }

    public object SaveData()
    {
        List<DynamicSaveableData> serializedItems = new List<DynamicSaveableData>();

        foreach (var item in _items)
        {
            serializedItems.Add(SaveLoadUtility.SerializeGameObject(item.gameObject));
        }

        return serializedItems;
    }

    public void LoadData(object data)
    {
        foreach (var serializedItem in data as List<DynamicSaveableData>)
        {
            Item item = SaveLoadUtility.DeserializeGameObject(serializedItem).GetComponent<Item>();
            AddItem(item);
        }
    }

}

public static class SaveLoadUtility
{
    public static DynamicSaveableData SerializeGameObject(GameObject gameObject)
    {
        DynamicSaveable dynamicSaveable = gameObject.GetComponent<DynamicSaveable>();

        return new DynamicSaveableData()
        {
            ResourcesPath = dynamicSaveable.PrefabPath,
            Guid = dynamicSaveable.SceneGuid,
            Position = dynamicSaveable.transform.position,
            Rotation = dynamicSaveable.transform.eulerAngles,
            Components = dynamicSaveable.GetComponent<SaveableComponents>().GatherData()
        };
    }

    public static GameObject DeserializeGameObject(DynamicSaveableData data)
    {
        string resourcePath = data.ResourcesPath;
        resourcePath = PrefabPathToResourcePath(resourcePath);

        DynamicSaveable dynamicSaveable = GameObject.Instantiate(Resources.Load<DynamicSaveable>(resourcePath));

        dynamicSaveable.SetSceneGuid(data.Guid);

        dynamicSaveable.transform.position = data.Position;
        dynamicSaveable.transform.eulerAngles = data.Rotation;

        dynamicSaveable.GetComponent<SaveableComponents>().RestoreState(data.Components);

        return dynamicSaveable.gameObject;
    }

    private static string PrefabPathToResourcePath(string path)
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

}
