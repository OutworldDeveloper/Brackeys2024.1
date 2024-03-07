public sealed class PickupMovableInteraction : Interaction
{
    public override string Text => $"Pickup {GetComponent<Movable>().DisplayName}";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return player.Grip.IsHolding == false;
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Grip.PickUp(GetComponent<Movable>());
    }

}
