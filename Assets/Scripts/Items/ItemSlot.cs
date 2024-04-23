using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ItemSlot
{

    public event Action<ItemSlot> Changed;

    public readonly MonoBehaviour Owner;
    public readonly string SlotName;
    public readonly Type SlotType;

    private ItemStack _stack;

    public ItemSlot(MonoBehaviour owner, string name, Type slotType)
    {
        Owner = owner;
        SlotName = name;
        SlotType = slotType;
    }

    public ItemSlot(MonoBehaviour owner, string name) : this(owner, name, typeof(Item)) { }

    public IReadOnlyStack Stack => _stack;
    public bool IsEmpty => _stack == null; // || _stack.Count < 1;

    public ItemStack GetStack() // Should copy?
    {
        return _stack;
    }

    public bool IsCompatableWith(Item item)
    {
        return item.GetType().IsSubclassOf(SlotType) || item.GetType() == SlotType;
    }

    public bool CanAdd(ItemStack stack)
    {
        if (stack.Count <= 0)
            throw new Exception("Cannot add stack with an invalid amount of items.");

        if (IsCompatableWith(stack.Item) == false)
            return false;

        if (IsEmpty == true)
            return true;

        if (_stack.CanAdd(stack) == false)
            return false;

        return true;
    }

    public bool TryAdd(ItemStack stack)
    {
        if (stack.Count <= 0)
            throw new Exception("Cannot add stack with an invalid amount of items.");

        // Можно ли класть в этот слот предметы такого типа
        if (IsCompatableWith(stack.Item) == false)
            return false;

        // Есть ли уже предметы в слоте?
        if (IsEmpty == true)
        {
            _stack = stack;

            _stack.Changed += OnStackChanged;

            Changed?.Invoke(this);
            return true;
        }

        // Можно ли соеденить предметы в один стак
        if (_stack.CanAdd(stack) == false)
            return false;

        _stack.Add(stack);
        Changed?.Invoke(this);
        return true;
    }

    public ItemStack Take(int amount)
    {
        if (IsEmpty == true)
            throw new Exception("Cannot take from an empty slot");

        ItemStack result = _stack.Take(amount);

        //if (_stack.Count <= 0)
        //{
        //    _stack.Changed -= OnStackChanged;
        //    _stack = null;
        //}

        Changed?.Invoke(this);
        return result;
    }

    private void OnStackChanged()
    {
        if (_stack.Count <= 0)
        {
            _stack.Changed -= OnStackChanged;
            _stack = null;
        }

        Changed?.Invoke(this);
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append($"Slot [{SlotName}] ");

        stringBuilder.Append("(");

        if (IsEmpty == false)
            stringBuilder.Append(Stack.ToString());
        else
            stringBuilder.Append("Empty");

        stringBuilder.Append(")");

        return stringBuilder.ToString();
    }

}
