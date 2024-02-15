using UnityEngine;

public sealed class KnockDoorInteraction : Interaction
{

    [SerializeField] private Door _door;

    public override string Text => "Knock";

    public override void Perform(PlayerCharacter player)
    {
        _door.Knock();
    }
}
