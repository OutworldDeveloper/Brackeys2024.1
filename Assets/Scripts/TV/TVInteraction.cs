using UnityEngine;

public sealed class TVInteraction : Interaction
{

    [SerializeField] private TV _target;

    public override string Text => "Watch presentation";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _target.IsPlayingSequence == false;
    }

    public override void Perform(PlayerCharacter player)
    {
        _target.StartSequence();
    }

}
