using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class DynamicSaveable : MonoBehaviour
{

    [SerializeField] private GameObject _selfPrefab;
    [SerializeField] private string _prefabPath;
    [SerializeField] private bool _hasSceneID;
    [SerializeField] private string _sceneID;

    public string PrefabPath => _prefabPath;
    public string SceneGuid => _sceneID;

    [SerializeField] private SaveableComponentInfo[] _saveableComponentInfos = Array.Empty<SaveableComponentInfo>();

    public void SetSceneGuid(string guid)
    {
        _hasSceneID = true;
        _sceneID = guid;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (gameObject.scene == default)
        {
            _hasSceneID = false;
            _sceneID = string.Empty;
        }

        if (Application.isPlaying == true)
            return;

        if (gameObject.scene == default)
        {
            _selfPrefab = PrefabUtility.FindPrefabRoot(gameObject);
        }
        else
        {
            _selfPrefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

            if (_hasSceneID == false)
            {
                _sceneID = Guid.NewGuid().ToString();
                _hasSceneID = true;
            }
        }

        _prefabPath = AssetDatabase.GetAssetPath(_selfPrefab);

        //
        foreach (var saveableComponentInfo in _saveableComponentInfos)
        {
            if (saveableComponentInfo.Guid == string.Empty)
            {
                saveableComponentInfo.Guid = Guid.NewGuid().ToString();
            }
        }
    }
#endif

    private void OnEnable()
    {
        if (Application.isPlaying == false)
            return;

        if (_hasSceneID == true)
            return;

        _sceneID = Guid.NewGuid().ToString();
        _hasSceneID = true;
    }

    public Dictionary<string, object> GatherData()
    {
        var container = new Dictionary<string, object>();

        foreach (var monoBehaviour in gameObject.GetComponentsInChildren<MonoBehaviour>())
        {
            GatherComponentData(monoBehaviour, container);
        }

        Debug.Log($"Data is gathered, results:");

        foreach (var item in container)
        {
            Debug.Log($"{item.Key}: {item.Value}");
            Debug.Log($"Type: {item.Value.GetType()}");
        }

        return container;
    }

    private void GatherComponentData(MonoBehaviour target, Dictionary<string, object> data)
    {
        Type type = target.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (var field in type.GetFields(flags))
        {
            bool isPersistent = field.GetCustomAttribute<PersistentAttribute>() != null;

            if (isPersistent == false)
                continue;

            object value = field.GetValue(target);

            if (field.FieldType == typeof(Vector3))
            {
                value = new SerializableVector3((Vector3)value);
            }

            data.Add($"{type.Name}.{field.Name}", value);
            Debug.Log($"{field.Name} is a persistent field. We will save it's value {field.GetValue(target)}.");
        }
    }

    public void RestoreState(Dictionary<string, object> data)
    {
        foreach (var monoBehaviour in gameObject.GetComponentsInChildren<MonoBehaviour>())
        {
            RestoreComponentData(monoBehaviour, data);
        }
    }

    private void RestoreComponentData(MonoBehaviour target, Dictionary<string, object> data)
    {
        Type type = target.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (var field in type.GetFields(flags))
        {
            bool isPersistent = field.GetCustomAttribute<PersistentAttribute>() != null;

            if (isPersistent == false)
                continue;

            string key = $"{type.Name}.{field.Name}";

            if (data.ContainsKey(key) == false)
                continue;

            object value = data[key];

            if (value.GetType() == typeof(SerializableVector3))
            {
                value = (Vector3)(SerializableVector3)value;
            }
            else
            {
                value = Convert.ChangeType(value, field.FieldType);
            }

            field.SetValue(target, value);
        }
    }

}

[Serializable]
public sealed class SaveableComponentInfo
{
    public MonoBehaviour Component;
    public string Guid;

}

[AttributeUsage(AttributeTargets.Field)]
public class PersistentAttribute : Attribute { }

[Serializable]
public struct SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 original)
    {
        x = original.x;
        y = original.y;
        z = original.z;
    }

    public static implicit operator Vector3(SerializableVector3 from) => new Vector3(from.x, from.y, from.z);
    public static implicit operator SerializableVector3(Vector3 from) => new SerializableVector3(from);

}
