using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public sealed class DeathVolume : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;

    private Volume _volume;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
    }

    private void OnEnable()
    {
        _character.Died += OnCharacterDied;
    }

    private void OnDisable()
    {
        _character.Died -= OnCharacterDied;
    }

    private void OnCharacterDied()
    {
        Delayed.Do(() => _volume.weight = 1f, 0.1f);
    }

    private void OnCharacterRespawned()
    {
        _volume.weight = 0f;
    }

}
