public sealed class PlaceObjectInteraction : Interaction
{
    public override string Text => "Place object";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return player.Grip.IsHolding == true;
    }

    public override void Perform(PlayerCharacter player)
    {
        player.Grip.TryPlace();
    }

}