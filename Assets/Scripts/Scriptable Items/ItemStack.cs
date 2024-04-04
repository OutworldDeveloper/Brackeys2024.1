using System;

public sealed class ItemStack : IReadOnlyStack
{

    public ItemStack(ItemDefinition definition, int count = 1)
    {
        Item = definition;
        Data = new RuntimeItemData();
        Count = count;
    }

    public ItemStack(ItemDefinition definition, RuntimeItemData data, int count = 1) : this(definition, count)
    {
        Data = data;
    }

    public ItemDefinition Item { get; private set; }
    public RuntimeItemData Data { get; private set; }
    public int Count { get; private set; }

    public bool CanAdd(ItemStack other)
    {
        if (Count <= 0)
            return true;

        if (Item != other.Item)
            return false;

        if (Data.IsEmpty == false)
            return false;

        if (other.Data.IsEmpty == false)
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
        Data = stack.Data;
    }

    public ItemStack Take(int amount)
    {
        if (amount > Count)
        {
            throw new Exception("Invalid take count request");
        }

        Count -= amount;
        var result = new ItemStack(Item, Data.Copy(), amount);
        return result;
    }

    public override string ToString()
    {
        return $"{Item.DisplayName} ({Count})";
    }

}

public interface IReadOnlyStack
{
    public ItemDefinition Item { get; }
    public RuntimeItemData Data { get; }
    public int Count { get; }

}
