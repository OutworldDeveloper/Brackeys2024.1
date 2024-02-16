using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    public bool WantsUnpossess { get; private set; }
    public Player Player { get; private set; }

    public abstract Vector3 GetCameraPosition();
    public abstract Quaternion GetCameraRotation();
    public virtual void PossessedTick() { }

    public virtual void OnPossessed(Player player)
    {
        Player = player;
        WantsUnpossess = false;
    }

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
