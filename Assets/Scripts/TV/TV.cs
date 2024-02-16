using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public sealed class TV : MonoBehaviour
{

    private readonly int _screenTextureID = Shader.PropertyToID("_ScreenTex");
    private readonly int _noiseMultiplierID = Shader.PropertyToID("_NoiseMultiplier");

    [SerializeField] private Texture2D[] _textureSequence;
    [SerializeField] private Material _material;
    [SerializeField] private AudioSource _staticNoiseSource;
    [SerializeField] private float _delay = 0.5f;
    [SerializeField] private float _displayTime = 1f;
    [SerializeField] private float _transitionDuration = 1f;
    [SerializeField] private float _minNoiseValue = 0.6f;

    public bool IsPlayingSequence { get; private set; }

    private void Start()
    {
        _material.SetFloat(_noiseMultiplierID, 1f);
        _staticNoiseSource.volume = 1f;
    }

    public void StartSequence()
    {
        if (IsPlayingSequence == true)
            return;

        IsPlayingSequence = true;

        var sequence = DOTween.Sequence();

        foreach (var texture in _textureSequence)
        {
            sequence.Append(Show(texture));
            sequence.AppendInterval(_delay);
        }

        sequence.OnComplete(() => IsPlayingSequence = false);
    }

    private Sequence Show(Texture2D texture)
    {
        return DOTween.Sequence().
            AppendCallback(() => _material.SetTexture(_screenTextureID, texture)).

            Append(_material.DOFloat(_minNoiseValue, _noiseMultiplierID, _transitionDuration)).
            Join(_staticNoiseSource.DOFade(_minNoiseValue, _transitionDuration)).

            AppendInterval(_displayTime).

            Append(_material.DOFloat(1f, _noiseMultiplierID, _transitionDuration)).
            Join(_staticNoiseSource.DOFade(1f, _transitionDuration));
    }

}
