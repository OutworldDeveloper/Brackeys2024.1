using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

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

    [ContextMenu("Gather data")]
    public ComponentsData GatherData()
    {
       var componentsData = new ComponentsData();

        foreach (var saveableComponent in _saveableComponents)
        {
            var componentData = GatherComponentData(saveableComponent.Component);
            componentsData.Components.Add(saveableComponent.Guid, componentData);
        }

        Debug.Log($"Data is gathered, results:");

        foreach (var itemA in componentsData.Components)
        {
            Debug.Log($"{itemA.Key}");
            foreach (var itemB in itemA.Value.Fields)
            {
                Debug.Log($"{itemB.Key}: {itemB.Value}");
                Debug.Log($"Type: {itemB.Value.GetType()}");
            }
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

            if (field.FieldType == typeof(Vector3))
            {
                value = new SerializableVector3((Vector3)value);
            }

            componentData.Fields.Add($"{field.Name}", value);
            //Debug.Log($"{field.Name} is a persistent field. We will save it's value {field.GetValue(component)}.");
        }

        return componentData;
    }

    public void RestoreState(ComponentsData data)
    {
        foreach (var saveableComponent in _saveableComponents)
        {
            if (data.Components.TryGetValue(saveableComponent.Guid, out ComponentData componentData) == true)
            {
                RestoreComponentData(saveableComponent.Component, componentData.Fields);
            }
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

            string key = $"{field.Name}";

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

public class ComponentsData
{
    public Dictionary<string, ComponentData> Components = new Dictionary<string, ComponentData>();

}

public class ComponentData
{
    public Dictionary<string, object> Fields = new Dictionary<string, object>();
}
