﻿using System;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{

    [SerializeField] private VirtualCamera _virtualCamera;

    public bool WantsUnpossess { get; private set; }
    public Player Player { get; private set; }
    public virtual bool ShowCursor => false;
    protected VirtualCamera VirtualCamera => _virtualCamera;

    public virtual CameraState GetCameraState()
    {
        return _virtualCamera.State;
    }

    public virtual bool GetBlurStatus(out float targetDistance)
    {
        targetDistance = 0f;
        return false;
    }

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
