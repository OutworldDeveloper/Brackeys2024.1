using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryUI : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private ItemDisplayUI _itemDisplayPrefab;

    private readonly Dictionary<Item, ItemDisplayUI> _itemDisplays = new Dictionary<Item, ItemDisplayUI>();

    private void OnEnable()
    {
        _player.Inventory.ItemAdded += OnItemAdded;
        _player.Inventory.ItemRemoved += OnItemRemoved;
        Refresh();
    }

    private void OnDisable()
    {
        _player.Inventory.ItemAdded -= OnItemAdded;
        _player.Inventory.ItemRemoved -= OnItemRemoved;
    }

    private void OnItemAdded(Item item)
    {
        if (_itemDisplays.ContainsKey(item) == true) // Do not create visuals for already existing item
            return;

        var display = Instantiate(_itemDisplayPrefab, transform, false);
        display.Init(item);
        _itemDisplays.Add(item, display);
    }

    private void OnItemRemoved(Item item)
    {
        if (_itemDisplays.TryGetValue(item, out var display) == false)
            return;

        _itemDisplays.Remove(item);
        Destroy(display.gameObject);
    }

    private void Refresh()
    {
        foreach (var itemDisplay in _itemDisplays.Values)
        {
            Destroy(itemDisplay.gameObject);
        }

        _itemDisplays.Clear();

        foreach (var item in _player.Inventory.Items)
        {
            OnItemAdded(item);
        }
    }


}
