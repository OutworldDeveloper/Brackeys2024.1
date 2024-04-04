﻿using System;
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

    public ItemSlot(MonoBehaviour owner, string name) : this(owner, name, typeof(ItemDefinition)) { }

    public IReadOnlyStack Stack => _stack;
    public bool IsEmpty => _stack == null;

    public bool IsCompatableWith(ItemDefinition item)
    {
        return item.GetType().IsSubclassOf(SlotType) || item.GetType() == SlotType;
    }

    public bool TryAdd(ItemStack stack)
    {
        // Можно ли класть в этот слот предметы такого типа
        if (IsCompatableWith(stack.Item) == false)
            return false;

        // Есть ли уже предметы в слоте?
        if (IsEmpty == true)
        {
            _stack = stack;
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

        if (_stack.Count <= 0)
        {
            _stack = null;
        }

        Changed?.Invoke(this);
        return result;
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
