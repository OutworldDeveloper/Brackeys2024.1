using UnityEngine;

public sealed class Hole : MonoBehaviour
{

    [SerializeField] private Item _stickItem;
    [SerializeField] private Item _hookItem;
    [SerializeField] private PickupInteraction _stuckPickup;
    [SerializeField] private Animator _animator;

    public bool IsItemExtracted { get; private set; }

    private void Start()
    {
        _stuckPickup.enabled = false;
    }

    public bool TryExtractItem(PlayerCharacter player)
    {
        bool hasStick = player.Inventory.HasItem(_stickItem);
        bool hasHook = player.Inventory.HasItem(_hookItem);

        if (hasStick == false || hasHook == false)
        {
            Notification.Do(GetFailResponse(hasStick, hasHook), 1.5f);
            return false;
        }

        player.Inventory.RemoveItem(_stickItem);
        player.Inventory.RemoveItem(_hookItem);

        IsItemExtracted = true;
        Notification.Do("Worked", 1.5f);
        _stuckPickup.enabled = true;
        //_animator.Play("ExtractItem");
        //Delayed.Do()

        _stuckPickup.gameObject.SetActive(true);

        return true;
    }

    private string GetFailResponse(bool hasStick, bool hasHook)
    {
        if (hasStick == true && hasHook == false)
            return "I need a hook";

        if (hasStick == false && hasHook == true)
            return "I need a fishing rode";

        return "There is something interesting there";
    }

}
