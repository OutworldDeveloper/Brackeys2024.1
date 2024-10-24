using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnFadeVolume : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;

    private void OnEnable()
    {
        _character.Died += OnCharacterDied;
        _character.Respawned += OnCharacterRespawned;
    }

    private void OnDisable()
    {
        _character.Died -= OnCharacterDied;
        _character.Respawned -= OnCharacterRespawned;
    }

    private void OnCharacterDied(DeathType deathType)
    {
        Delayed.Do(() => ScreenFade.FadeOutFor(0.25f), Mathf.Max(_character.RespawnTime - 0.25f, 0.5f));
    }

    private void OnCharacterRespawned() { }

}
