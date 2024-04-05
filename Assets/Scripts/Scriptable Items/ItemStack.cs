using System;

[System.Serializable]
public sealed class ItemStack : IReadOnlyStack
{

    public ItemStack(Item definition, int count = 1)
    {
        Item = definition;
        Components = new ItemComponents();
        Item.CreateComponents(Components);
        Count = count;
    }

    public ItemStack(Item definition, ItemComponents components, int count = 1) : this(definition, count)
    {
        Components = components;
    }

    public Item Item { get; private set; }
    public ItemComponents Components { get; private set; }
    public int Count { get; private set; }

    public bool CanAdd(ItemStack other)
    {
        if (Count <= 0)
            return true;

        if (Item != other.Item)
            return false;

        if (Components.IsEmpty == false)
            return false;

        if (other.Components.IsEmpty == false)
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
        Components = stack.Components;
    }

    public ItemStack Take(int amount)
    {
        if (amount > Count)
        {
            throw new Exception("Invalid take count request");
        }

        Count -= amount;
        var result = new ItemStack(Item, Components.Copy(), amount);
        return result;
    }

    public override string ToString()
    {
        return $"{Item.DisplayName} ({Count})";
    }

}

public interface IReadOnlyStack
{
    public Item Item { get; }
    public ItemComponents Components { get; }
    public int Count { get; }

}
