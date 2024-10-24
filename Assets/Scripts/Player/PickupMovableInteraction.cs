public sealed class PickupMovableInteraction : Interaction
{
    public override string Text => "Pickup";

    public override void Perform(PlayerCharacter player)
    {
        Notification.ShowDebug("PickupMovableInteraction");
        player.Grip.PickUp(gameObject);
    }

}
