using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public sealed class HealthVolumeController : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;

    private Volume _volume;

    private void Awake()
    {
        _volume = GetComponent<Volume>();
    }

    private void OnEnable()
    {
        _character.Damaged += OnCharacterDamaged;
    }

    private void OnDisable()
    {
        _character.Damaged -= OnCharacterDamaged;
    }

    private void Update()
    {
        //_volume.weight = 1 - _character.Health / _character.MaxHealth;
    }

    private void OnCharacterDamaged()
    {
        _volume.weight = 1 - _character.Health / _character.MaxHealth;
    }

    private void OnCharacterRespawned()
    {
        _volume.weight = 0f;
    }

}
