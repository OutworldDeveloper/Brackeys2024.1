using UnityEngine;

public sealed class TimedLeverInteraction : Interaction
{

    [SerializeField] private TimedLever _timedLever;

    public override string Text => "Activate";

    public override bool IsAvaliable(PlayerCharacter player)
    {
        return _timedLever.IsActivated == false;
    }

    public override void Perform(PlayerCharacter player)
    {
        _timedLever.Activate();
    }

}
