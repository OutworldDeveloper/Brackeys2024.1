using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ShotgunTrap : MonoBehaviour
{

    [SerializeField] private Door _door;
    [SerializeField] private Sound _shootSound;
    [SerializeField] private AudioSource _shootAudioSource;
    [SerializeField] private PlayerTrigger _playerTrigger;
    [SerializeField] private Collider _doorBlocker;

    private bool IsDeactivated;

    public void Activate()
    {
        IsDeactivated = false;

        if (_door.IsOpen == true)
        {
            ShootAndCloseDoor();
        }

        _doorBlocker.enabled = true;
    }

    public void Deactivate()
    {
        IsDeactivated = true;
        _doorBlocker.enabled = false;
    }

    private void OnEnable()
    {
        _door.Opened += OnDoorOpened;
        _door.Opening += OnDoorOpening;
    }

    private void OnDisable()
    {
        _door.Opened -= OnDoorOpened;
        _door.Opening -= OnDoorOpening;
    }

    private void OnDoorOpening()
    {
        if (IsDeactivated == true)
            return;

        if (_playerTrigger.HasPlayerInside == false)
            return;

        _playerTrigger.PlayerInside.ApplyModifier(new ShotgunTrapModifier(), 1f);
    }

    private void OnDoorOpened()
    {
        if (IsDeactivated == false)
            ShootAndCloseDoor();
    }

    private void ShootAndCloseDoor()
    {
        if (_playerTrigger.HasPlayerInside == true)
            _playerTrigger.PlayerInside.Kill(DeathType.Physical);

        _shootSound.Play(_shootAudioSource);
        Delayed.Do(() => _door.Close(), 0.2f); // 0.7f
    }

}

public sealed class ShotgunTrapModifier : CharacterModifier
{
    public override float GetSpeedMultiplier()
    {
        return 0.5f;
    }

    public override bool CanInteract()
    {
        return false;
    }

    public override bool CanJump()
    {
        return false;
    }

}
