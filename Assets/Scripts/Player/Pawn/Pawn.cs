using System.Collections.Generic;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{

    [SerializeField] private VirtualCamera _virtualCamera;
    [SerializeField] private CameraTransition _transition;

    private readonly List<PawnAction> _actions = new List<PawnAction>();

    public Player Player { get; private set; }
    public virtual bool ShowCursor => false;
    public bool HasActions => _actions.Count > 0;
    protected VirtualCamera VirtualCamera => _virtualCamera;
    public bool IsPossesed => Player != null && Player.PawnStack.ActivePawn == this;
    public CameraTransition CameraTransition => _transition;

    public virtual CameraState GetCameraState()
    {
        return _virtualCamera.State;
    }

    public virtual bool GetBlurStatus(out float targetDistance)
    {
        targetDistance = 0f;
        return false;
    }

    public virtual void OnAddedToStack(Player player)
    {
        Player = player;
    }

    public virtual void OnRemovedFromStack(Player player) { }

    public void RemoveFromStack()
    {
        Player.PawnStack.Remove(this);
    }

    public virtual void OnReceivePlayerControl() { }
    public virtual void OnLostPlayerControl() { }

    public virtual void InputTick() { }

    public virtual bool CanRemoveAtWill()
    {
        return true;
    }

    public PawnAction[] GetActions()
    {
        return _actions.ToArray();
    }

    protected void RegisterAction(PawnAction action)
    {
        _actions.Add(action);
    }

}

public enum CameraTransition
{
    None,
    Fade,
    Move
}