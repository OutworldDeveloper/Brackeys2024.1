public sealed class PlaceObjectInteraction : Interaction
{
    public override string Text => "Place";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return player.Grip.IsHolding == true && player.Grip.CanPlace == true;
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Grip.TryPlace();
    }

}