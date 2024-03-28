using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;

public sealed class SaveableComponents : MonoBehaviour
{

    [SerializeField] private List<SaveableComponentInfo> _saveableComponents = new();

    public void TryAdd(MonoBehaviour monoBehaviour)
    {
        bool isPartOfObject = false;

        foreach (var other in GetComponentsInChildren<MonoBehaviour>())
        {
            if (other == monoBehaviour)
            {
                isPartOfObject = true;
            }
        }

        if (isPartOfObject == false)
            return;

        if (_saveableComponents.Any(t => t.Component == monoBehaviour) == true)
            return;

        _saveableComponents.Add(new SaveableComponentInfo()
        {
            Guid = Guid.NewGuid().ToString(),
            Component = monoBehaviour,
        });
    }

    public void RemoveAt(int index)
    {
        _saveableComponents.RemoveAt(index);
    }

    public MonoBehaviour[] GetAll()
    {
        var array = new MonoBehaviour[_saveableComponents.Count];

        for (int i = 0; i < array.Length; i++)
        {
            array[i] = _saveableComponents[i].Component;
        }

        return array;
    }

    [ContextMenu("Gather data")]
    public ComponentsData GatherData()
    {
       var componentsData = new ComponentsData();

        foreach (var saveableComponent in _saveableComponents)
        {
            var componentData = GatherComponentData(saveableComponent.Component);
            componentsData.Components.Add(saveableComponent.Guid, componentData);
        }

        return componentsData;
    }

    private ComponentData GatherComponentData(MonoBehaviour component)
    {
        var componentData = new ComponentData();

        Type type = component.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (var field in type.GetFields(flags))
        {
            bool isPersistent = field.GetCustomAttribute<PersistentAttribute>() != null;

            if (isPersistent == false)
                continue;

            object value = field.GetValue(component);
            componentData.Fields.Add(field.Name, value);
        }

        // Custom data
        if (component is ICustomSaveable customSaveable)
        {
            componentData.CustomData = customSaveable.SaveData();
        }

        return componentData;
    }

    public void RestoreState(ComponentsData data)
    {
        foreach (var saveableComponent in _saveableComponents)
        {
            if (data.Components.TryGetValue(saveableComponent.Guid, out ComponentData componentData) == true)
            {
                RestoreComponentData(saveableComponent.Component, componentData);
            }
        }
    }

    private void RestoreComponentData(MonoBehaviour target, ComponentData data)
    {
        Type type = target.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (var field in type.GetFields(flags))
        {
            bool isPersistent = field.GetCustomAttribute<PersistentAttribute>() != null;

            if (isPersistent == false)
                continue;

            string key = field.Name;

            if (data.Fields.ContainsKey(key) == false)
                continue;

            object value = data.Fields[key];
            field.SetValue(target, value);
        }

        // Custom data
        if (target is ICustomSaveable customSaveable)
        {
            customSaveable.LoadData(data.CustomData);
        }
    }

}

[AttributeUsage(AttributeTargets.Field)]
public class PersistentAttribute : Attribute { }

[Serializable]
public sealed class SaveableComponentInfo
{
    public MonoBehaviour Component;
    public string Guid;

}

[Serializable]
public class ComponentsData
{
    public Dictionary<string, ComponentData> Components = new Dictionary<string, ComponentData>();

}

[Serializable]
public class ComponentData
{
    public Dictionary<string, object> Fields = new Dictionary<string, object>();
    public object CustomData;

}

public interface ICustomSaveable
{
    public object SaveData();
    public void LoadData(object data);

}
