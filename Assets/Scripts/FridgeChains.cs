using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class FridgeChains : MonoBehaviour
{

    [SerializeField] private Door _door;
    [SerializeField] private Item _key;
    [SerializeField] private Sound _openingAttemptSound;
    [SerializeField] private Sound _unlockSound;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private GameObject _lockedGameObject;
    [SerializeField] private GameObject _unlockedGameObject;

    public bool IsUnlocked { get; private set; }

    private void OnEnable()
    {
        _door.OpeningAttempt += OnOpeningAttempt;
    }

    private void OnDisable()
    {
        _door.OpeningAttempt -= OnOpeningAttempt;
    }

    private void Start()
    {
        _door.Block();
        _lockedGameObject.SetActive(true);
        _unlockedGameObject.SetActive(false);
    }

    public void TryUnlock(PlayerCharacter player)
    {
        if (player.Inventory.HasItem(_key) == false)
            return;

        player.Inventory.RemoveItem(_key);
        _door.Unblock();
        IsUnlocked = true;
        _unlockSound.Play(_audioSource);
        _lockedGameObject.SetActive(false);
        _unlockedGameObject.SetActive(true);
        return;
    }

    private void OnOpeningAttempt(PlayerCharacter player, bool success)
    {
        if (IsUnlocked == false)
            _openingAttemptSound.Play(_audioSource);
    }

}
