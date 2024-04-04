using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class ItemDefinition : ScriptableObject
{

    [field: Header("Item")]
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public int StackSize { get; private set; } = 1;

    public virtual ItemStack Create(int count)
    {
        return new ItemStack(this, count);
    }

}

public class BoxItemDefinition : ItemDefinition
{

    [field: Header("Box")]
    [field: SerializeField] public ItemDefinition Reward { get; private set; }

}

[Serializable]
public sealed class RuntimeItemData
{

    private readonly Dictionary<Type, ItemComponent> _components = new Dictionary<Type, ItemComponent>();

    public RuntimeItemData() { }

    public bool IsEmpty => _components.Keys.Count == 0;

    public T GetComponent<T>() where T : ItemComponent
    {
        if (_components.ContainsKey(typeof(T)) == false)
            return default;

        return (T)_components[typeof(T)];
    }

    public void SetComponent(ItemComponent component)
    {
        _components.AddOrUpdate(component.GetType(), component);
    }

    public RuntimeItemData Copy()
    {
        var copy = new RuntimeItemData();

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

public static class Order
{
    public const int UI = 10000;

}

public static class Items
{

    private static Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadItems()
    {
        foreach (var itemDefinition in Resources.LoadAll<ItemDefinition>(string.Empty))
        {
            _items.Add(itemDefinition.name, itemDefinition);
        }
    }

    public static ItemDefinition Get(string id)
    {
        return _items[id];
    }

}
