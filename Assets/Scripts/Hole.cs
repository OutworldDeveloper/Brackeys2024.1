using UnityEngine;

public sealed class Hole : MonoBehaviour
{

    [SerializeField] private Item _stickItem;
    [SerializeField] private Item _hookItem;
    [SerializeField] private PickupInteraction _stuckPickup;
    [SerializeField] private Animator _animator;

    public bool IsItemExtracted { get; private set; }

    public bool TryExtractItem(PlayerCharacter player)
    {
        if (player.Inventory.HasItem(_stickItem) == false)
        {
            Notification.Do("I can't reach it", 1.5f);
            return false;
        }

        if (player.Inventory.HasItem(_hookItem) == false)
        {
            Notification.Do("I need a hook or something", 1.5f);
            return false;
        }

        player.Inventory.RemoveItem(_stickItem);
        player.Inventory.RemoveItem(_hookItem);

        IsItemExtracted = true;
        Notification.Do("Worked", 1.5f);
        //_animator.Play("ExtractItem");
        //Delayed.Do()
        return true;
    }

}
