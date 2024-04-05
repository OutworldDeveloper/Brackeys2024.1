using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{

    [field: Header("Item")]
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public int StackSize { get; private set; } = 1;

    [SerializeField, HideInInspector] private List<ItemTag> _tags = new List<ItemTag>();

    public virtual ItemStack Create(int count)
    {
        return new ItemStack(this, count);
    }

    public virtual void CreateComponents(ItemComponents components) { }

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

[Serializable]
public sealed class ItemComponents
{

    private readonly Dictionary<Type, ItemComponent> _components = new Dictionary<Type, ItemComponent>();

    public ItemComponents() { }

    public bool IsEmpty => _components.Keys.Count == 0;

    public T Get<T>() where T : ItemComponent
    {
        if (_components.ContainsKey(typeof(T)) == false)
            return default;

        return (T)_components[typeof(T)];
    }

    public void Add(ItemComponent component)
    {
        _components.Add(component.GetType(), component);
    }

    public bool Has<T>(out T component) where T : ItemComponent
    {
        if (_components.TryGetValue(typeof(T), out ItemComponent result))
        {
            component = result as T;
            return true;
        }

        component = null;
        return false;
    }

    public bool Has<T>() where T : ItemComponent
    {
        return Has(out T component);
    }

    public ItemComponents Copy()
    {
        var copy = new ItemComponents();

        foreach (var key in _components.Keys)
        {
            var component = _components[key];
            copy._components.Add(key, component.Copy());
        }

        return copy;
    }

}

[Serializable]
public abstract class ItemComponent
{
    public abstract ItemComponent Copy();

}

[Serializable]
public sealed class LoadedAmmoComponent : ItemComponent
{
    public int Value;

    public override ItemComponent Copy()
    {
        return new LoadedAmmoComponent()
        {
            Value = Value,
        };
    }

}
