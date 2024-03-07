using UnityEngine;

public class PickupInteraction : Interaction
{

    [SerializeField] private Item _item;

    public override string Text => $"Pickup {_item.DisplayName}";

    public override void Perform(PlayerCharacter player)
    {
        player.Inventory.AddItem(_item);
    }

}
