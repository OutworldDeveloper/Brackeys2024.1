using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoviePawn : Pawn
{

    [SerializeField] private float _duration = 2f;

    private TimeUntil _timeUntilUnpossess;

    public void SetDuration(float duration)
    {
        _duration = duration;
    }

    public override bool CanRemoveAtWill()
    {
        return false;
    }

    public override void OnReceivePlayerControl()
    {
        base.OnReceivePlayerControl();
        _timeUntilUnpossess = new TimeUntil(Time.time + _duration);
    }

    private void Update()
    {
        if (IsPossesed == false)
            return;

        if (_timeUntilUnpossess > 0f)
            return;

        RemoveFromStack();
    }

}
