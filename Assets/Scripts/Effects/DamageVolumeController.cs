using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public sealed class DamageVolumeController : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;

    private Volume _volume;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
    }

    private void OnEnable()
    {
        _character.Damaged += OnCharacterDied;
    }

    private void OnDisable()
    {
        _character.Damaged -= OnCharacterDied;
    }

    private void OnCharacterDied()
    {
        _volume.weight = 1f;
        Delayed.Do(() => _volume.weight = 0f, 0.25f);
    }

}
