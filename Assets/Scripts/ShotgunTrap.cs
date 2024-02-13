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

    private bool IsDeactivated;

    public void Activate()
    {
        IsDeactivated = false;
    }

    public void Deactivate()
    {
        IsDeactivated = true;
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

        _playerTrigger.PlayerInside.ApplyModifier(new ShotgunTrapModifier(), 5f);
    }

    private void OnDoorOpened()
    {
        if (IsDeactivated == false)
            Shoot();
    }

    private void Shoot()
    {
        if (_playerTrigger.HasPlayerInside == true)
            _playerTrigger.PlayerInside.Kill(DeathType.Physical);

        _shootSound.Play(_shootAudioSource);
        Delayed.Do(() => _door.Close(), 0.2f); // 0.7f
    }

}

public sealed class ShotgunTrapModifier : CharacterModifier
{
    public override float GetSpeedMultipler()
    {
        return 0.1f;
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
