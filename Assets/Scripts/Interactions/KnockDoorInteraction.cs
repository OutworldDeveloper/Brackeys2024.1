using UnityEngine;

public sealed class KnockDoorInteraction : Interaction
{

    [SerializeField] private Door _door;

    public override string Text => "Knock";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _door.IsOpen == false;
    }

    public override void Perform(PlayerCharacter player)
    {
        _door.Knock();
    }
}
