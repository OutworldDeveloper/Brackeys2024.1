using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class FridgeChains : MonoBehaviour
{

    [SerializeField] private Door _door;
    [SerializeField] private Item _key;

    public bool IsLocked { get; private set; }

    private void Start()
    {
        _door.Block();
    }

    public void TryUnlock(PlayerCharacter player)
    {
        if (player.Inventory.HasItem(_key) == false)
            return;

        player.Inventory.RemoveItem(_key);
        _door.Unblock();
        return;
    }

}
