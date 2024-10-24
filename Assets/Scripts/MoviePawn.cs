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

    public override bool CanUnpossessAtWill()
    {
        return false;
    }

    public override void OnPossessed(Player player)
    {
        base.OnPossessed(player);
        _timeUntilUnpossess = new TimeUntil(Time.time + _duration);
    }

    public override void PossessedTick()
    {
        if (_timeUntilUnpossess < 0f)
        {
            Unpossess();
        }
    }

    public override Vector3 GetCameraPosition()
    {
        return transform.position;
    }

    public override Quaternion GetCameraRotation() => transform.rotation;

}
