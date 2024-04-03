using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ItemBoxInteraction : Interaction
{

    [SerializeField] private ExpInventory _containerInventory;
    [SerializeField] private Prefab<UI_InventoryAndContainerScreen> _containerScreen;

    public override string Text => "Open";

    public override void Perform(PlayerCharacter player)
    {
        var containerScreen = player.Player.Panels.InstantiateAndOpenFrom(_containerScreen);
        containerScreen.SetTarget(player);
        containerScreen.SetContainer(_containerInventory);
    }

}
