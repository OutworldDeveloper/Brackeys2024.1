using System;

[System.Serializable]
public sealed class ItemStack : IReadOnlyStack
{

    public event Action Changed;

    public ItemStack(Item definition, int count = 1)
    {
        Item = definition;
        Attributes = new ItemAttributes();
        Item.CreateAttributes(Attributes);
        Count = count;
    }

    public ItemStack(Item definition, ItemAttributes attributes, int count = 1) : this(definition, count)
    {
        Attributes = attributes;
    }

    public Item Item { get; private set; }
    public ItemAttributes Attributes { get; private set; }
    public int Count { get; private set; }


    public bool CanAdd(ItemStack other)
    {
        if (Count <= 0)
            return true;

        if (Item != other.Item)
            return false;

        if (Attributes.IsEmpty == false)
            return false;

        if (other.Attributes.IsEmpty == false)
            return false;

        if (Count + other.Count > Item.StackSize)
            return false;

        return true;
    }

    public void Add(ItemStack stack)
    {
        if (CanAdd(stack) == false)
            throw new Exception("Trying to add an incompatable stack");

        Count += stack.Count;
        Item = stack.Item;
        Attributes = stack.Attributes;

        Changed?.Invoke();
    }

    public ItemStack Take(int amount)
    {
        if (amount > Count)
        {
            throw new Exception("Invalid take count request");
        }

        Count -= amount;
        var result = new ItemStack(Item, Attributes.Copy(), amount);

        Changed?.Invoke();

        return result;
    }

    public T GetAttribute<T>(ItemAttribute<T> attribute) where T : struct
    {
        return Attributes.Get(attribute);
    }

    public void SetAttribute<T>(ItemAttribute<T> attribute, T value) where T : struct
    {
        Attributes.Set(attribute, value);
        Changed?.Invoke();
    }

    public bool HasAttribute<T>(ItemAttribute<T> attribute) where T : struct
    {
        return Attributes.Has(attribute);
    }

    public override string ToString()
    {
        return $"{Item.DisplayName} ({Count})";
    }

}

public interface IReadOnlyStack
{
    public Item Item { get; }
    public ItemAttributes Attributes { get; }
    public int Count { get; }
    public T GetAttribute<T>(ItemAttribute<T> attribute) where T : struct;
    public void SetAttribute<T>(ItemAttribute<T> attribute, T value) where T : struct;
    public bool HasAttribute<T>(ItemAttribute<T> attribute) where T : struct;

}
