using UnityEngine;

public class PickupInteraction : Interaction
{

    [SerializeField] private ItemDefinition _item;
    [SerializeField] private int _amount;

    public override string Text => $"Pickup {_item.DisplayName}";

    public override void Perform(PlayerCharacter player)
    {
        player.Inventory.TryAdd(new ItemStack(_item, _amount));
    }

}
