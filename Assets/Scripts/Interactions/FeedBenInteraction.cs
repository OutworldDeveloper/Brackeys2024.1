using UnityEngine;

public sealed class FeedBenInteraction : Interaction
{

    [SerializeField] private Ben _ben;

    public override string Text => "Give food";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _ben.IsHungry == true && player.Inventory.HasItemWithTag(_ben.FoodTag);
    }

    public override void Perform(PlayerCharacter player)
    {
        if (_ben.TryFeed(player) == false)
        {
            Notification.Show("I don't have any food...");
        }
    }
}

