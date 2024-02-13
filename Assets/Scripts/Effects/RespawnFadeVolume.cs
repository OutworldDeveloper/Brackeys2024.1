using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[SerializeField]
public class RespawnFadeVolume : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _character;
    [SerializeField] private float _fadeTime = 0.7f;

    private Volume _volume;
    private Tween _currentTween;

    private void OnEnable()
    {
        _volume = GetComponent<Volume>();
        _character.Died += OnCharacterDied;
        _character.Respawned += OnCharacterRespawned;
    }

    private void OnDisable()
    {
        _character.Died -= OnCharacterDied;
        _character.Respawned -= OnCharacterRespawned;
    }

    private void Start()
    {
        FadeIn();
    }

    private void OnCharacterDied(DeathType deathType)
    {
        Delayed.Do(FadeOut, Mathf.Max(_character.RespawnTime - 0.25f, 0.5f));
    }

    private void OnCharacterRespawned()
    {
        FadeIn();
    }

    private void FadeOut()
    {
        _currentTween?.Kill(true);
        //_currentTween = _mat.DOFloat(1f, _id, _fadeTime).From(0f);
        _currentTween = DOTween.To(() => _volume.weight, value => _volume.weight = value, 1f, _fadeTime).From(0f);
    }

    private void FadeIn()
    {
        _currentTween?.Kill(true);
        _currentTween = DOTween.To(() => _volume.weight, value => _volume.weight = value, 0f, _fadeTime).From(1f);
        //_currentTween = _mat.DOFloat(0f, _id, _fadeTime).From(1f);
    }

}
