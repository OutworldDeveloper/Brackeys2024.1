using UnityEngine;

public class PickupInteraction : Interaction
{

    [SerializeField] private Item _item;
    [SerializeField] private int _amount = 1;

    private ItemStack _stack;
    private string _text;

    [Persistent] private bool _isTaken;

    public override string Text => _text;

    private void Start()
    {
        if (_isTaken == true)
            gameObject.SetActive(false);

        // If Difficulty != _pickupDifficulty
        //  gameObject.SetActive(false)

        _stack = new ItemStack(_item, _amount);
        _text = $"Pickup {_item.DisplayName}" + (_amount > 1 ? $" ({_amount})" : string.Empty);
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
