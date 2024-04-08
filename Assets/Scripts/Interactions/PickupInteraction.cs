using UnityEngine;

public class PickupInteraction : Interaction
{

    [SerializeField] private Item _item;
    [SerializeField] private int _amount = 1;

    private ItemStack _stack;

    [Persistent] private bool _isTaken;

    public override string Text => $"Pickup {_item.DisplayName} ({_amount})";

    private void Start()
    {
        if (_isTaken == true)
            gameObject.SetActive(false);

        _stack = new ItemStack(_item, _amount);
    }

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return player.Inventory.CanAdd(_stack);
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Inventory.TryAdd(_stack);
        _isTaken = true;
        gameObject.SetActive(false);
    }

}
