using UnityEngine;

public class PickupInteraction : Interaction
{

    [SerializeField] private Item _item;
    [SerializeField] private int _amount = 1;

    public override string Text => $"Pickup {_item.DisplayName}";

    public override void Perform(PlayerCharacter player)
    {
        player.Inventory.TryAdd(new ItemStack(_item, _amount));
        Destroy(gameObject);
    }

}
