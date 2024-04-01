using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class ItemDefinition : ScriptableObject
{
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public Sprite WeaponSprite { get; private set; } // Maybe in weapon class?

    [SerializeReference] private ItemComponent[] _defaultComponents;

    public bool ContainsComponentByDefault(Type componentType)
    {
        foreach (var component in _defaultComponents)
        {
            if (component.GetType() == componentType)
                return true;
        }

        return false;
    }

    public ItemStack Create()
    {
        var stack = new ItemStack(this, 1);

        foreach (var itemComponent in _defaultComponents)
        {
            stack.Data.SetCompnoent(itemComponent.Copy());
        }

        return stack;
    }

}

[Serializable]
public sealed class RuntimeItemData
{

    private readonly Dictionary<Type, ItemComponent> _components = new Dictionary<Type, ItemComponent>();

    public RuntimeItemData() { }

    private RuntimeItemData(RuntimeItemData copyFrom)
    {
        foreach (var key in copyFrom._components.Keys)
        {
            _components.Add(key, _components[key]);
        }
    }

    public T GetComponent<T>() where T : ItemComponent
    {
        if (_components.ContainsKey(typeof(T)) == false)
            return default;

        return (T)_components[typeof(T)];
    }

    public void SetCompnoent(ItemComponent component)
    {
        _components.AddOrUpdate(component.GetType(), component);
    }

    public bool AreIdenticalWith(RuntimeItemData other)
    {
        // If we have different amount of components then we're not identical
        // Может если компоненты есть то уже не стоит стакаться?
        if (_components.Keys.Count != other._components.Keys.Count)
            return false;

        foreach (var componentType in _components.Keys)
        {
            if (other._components.ContainsKey(componentType) == false)
                return false;

            ItemComponent componentA = _components[componentType];
            ItemComponent componentB = other._components[componentType];

            if (componentA.AreIdenticalWith(componentB) == false)
                return false;
        }

        return true;
    }

    public RuntimeItemData Copy()
    {
        return new RuntimeItemData(this);
    }

}

[Serializable]
public sealed class ItemStack
{

    public event Action Updated;

    public readonly ItemDefinition Definition;
    public readonly RuntimeItemData Data;

    public ItemStack(ItemDefinition definition, int count = 1)
    {
        Definition = definition;
        Data = new RuntimeItemData();
        Count = count;
    }

    public ItemStack(ItemDefinition definition, RuntimeItemData data, int count = 1) : this(definition, count)
    {
        Data = data;
    }

    public int Count { get; private set; }

    public bool TryAddToStack(ItemStack other)
    {
        if (Definition != other.Definition)
            return false;

        if (Data.AreIdenticalWith(other.Data) == false)
            return false;

        Count += other.Count;

        Updated?.Invoke();
        return true;
    }

    public ItemStack Take(int takeCount)
    {
        if (takeCount < Count - 1)
        {
            throw new Exception("Invalid take count request");
        }

        Count = takeCount;
        var copy = new ItemStack(Definition, Data.Copy(), takeCount);

        Updated?.Invoke();
        return copy;
    }

}

[Serializable]
public abstract class ItemComponent
{
    public abstract ItemComponent Copy();
    public abstract bool AreIdenticalWith(ItemComponent other);

}

[Serializable]
public abstract class ItemComponentAuthoring
{
    public abstract ItemComponent CreateComponent();

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

    public override bool AreIdenticalWith(ItemComponent other)
    {
        return Value == (other as LoadedAmmoComponent).Value;
    }

}

[Serializable]
public sealed class InventoryComponent : ItemComponent
{
    public int Size;
    public ItemStack[] Items;

    public override ItemComponent Copy()
    {
        return new InventoryComponent()
        {
            Size = Size,
            Items = Items.ToArray(),
        };
    }

    public override bool AreIdenticalWith(ItemComponent other)
    {
        return false; // Впадлу думать
    }

}


public static class Order
{
    public const int UI = 10000;

}
