using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class FridgeChains : DoorBlocker
{

    [SerializeField] private ItemTag _keyTag;
    [SerializeField] private Sound _openingAttemptSound;
    [SerializeField] private Sound _unlockSound;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private GameObject _lockedGameObject;
    [SerializeField] private GameObject _unlockedGameObject;

    public bool IsUnlocked { get; private set; }

    private void Start()
    {
        _lockedGameObject.SetActive(true);
        _unlockedGameObject.SetActive(false);
    }

    public override bool IsActive() => IsUnlocked == false;
    public override string GetBlockReason() => "Chains block the door...";
    public override bool HasCustomSound() => true;
    public override Sound GetCustomSound() => _openingAttemptSound;

    public void TryUnlock(PlayerCharacter player)
    {
        if (IsUnlocked == true)
            return;

        IsUnlocked = true;
        _unlockSound.Play(_audioSource);

        _lockedGameObject.SetActive(false);
        _unlockedGameObject.SetActive(true);
    }

}
