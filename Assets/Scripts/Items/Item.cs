using Alchemy.Inspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{

    [field: SerializeField, FoldoutGroup(nameof(Item))] public string DisplayName { get; private set; }
    [field: SerializeField, FoldoutGroup(nameof(Item))] public string Description { get; private set; }
    [field: SerializeField, FoldoutGroup(nameof(Item))] public Sprite Sprite { get; private set; }
    [field: SerializeField, FoldoutGroup(nameof(Item))] public int StackSize { get; private set; } = 1;

    [SerializeField, HideInInspector] private List<ItemTag> _tags = new List<ItemTag>();

    public virtual void CreateAttributes(ItemAttributes attributes) { }

    public bool HasTag<T>(out T tag) where T : ItemTag
    {
        foreach (var inspectedTag in _tags)
        {
            if (inspectedTag is T)
            {
                tag = inspectedTag as T;
                return true;
            }
        }

        tag = default;
        return false;
    }

}

public sealed class ItemAttribute<T> where T : struct
{

    public readonly string Id;
    public readonly Type AttributeType;

    public ItemAttribute(string id)
    {
        Id = id;
        AttributeType = typeof(T);
    }

}

[Serializable]
public sealed class ItemAttributes
{

    private readonly Dictionary<string, object> _attributes = new Dictionary<string, object>();

    public ItemAttributes() { }

    public bool IsEmpty => _attributes.Keys.Count == 0;

    public T Get<T>(ItemAttribute<T> attribute) where T : struct
    {
        return (T)_attributes[attribute.Id];
    }

    public void Set<T>(ItemAttribute<T> attribute, T value) where T : struct
    {
        _attributes.Update(attribute.Id, value);
    }

    public bool Has<T>(ItemAttribute<T> attribute) where T : struct
    {
        return _attributes.ContainsKey(attribute.Id);
    }

    public ItemAttributes Copy()
    {
        var copy = new ItemAttributes();

        foreach (var key in _attributes.Keys)
        {
            var value = _attributes[key];
            copy._attributes.Add(key, value);
        }

        return copy;
    }

}
