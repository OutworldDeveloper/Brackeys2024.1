using System;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    public bool WantsUnpossess { get; private set; }
    public Player Player { get; private set; }
    public virtual bool OverrideCameraPositionAndRotation => true;
    public virtual bool OverrideCameraFOV => false;
    public virtual bool ShowCursor => false;

    public virtual Vector3 GetCameraPosition() => Vector3.zero;
    public virtual Quaternion GetCameraRotation() => Quaternion.identity;
    public virtual float GetCameraFOV() => throw new NotImplementedException();

    public virtual void PossessedTick() { }

    public virtual void OnPossessed(Player player)
    {
        Player = player;
        WantsUnpossess = false;
    }

    public virtual void InputTick() { }

    public virtual void OnUnpossessed()
    {
        Player = null;
    }

    protected void Unpossess()
    {
        WantsUnpossess = true;
    }

    public virtual bool CanUnpossessAtWill()
    {
        return true;
    }

}
