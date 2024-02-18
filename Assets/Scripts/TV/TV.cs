using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public sealed class TV : MonoBehaviour
{

    private readonly int _screenTextureID = Shader.PropertyToID("_ScreenTex");
    private readonly int _noiseMultiplierID = Shader.PropertyToID("_NoiseMultiplier");

    [SerializeField] private Material _material;
    [SerializeField] private AudioSource _staticNoiseSource;
    [SerializeField] private float _delay = 0.5f;
    [SerializeField] private float _displayTime = 1f;
    [SerializeField] private float _transitionDuration = 1f;
    [SerializeField] private float _minNoiseValue = 0.6f;
    [SerializeField] private PlayerTrigger _roomTrigger;
    [SerializeField] private Sound _startSound;
    [SerializeField] private AudioSource _startAudioSource;

    [SerializeField] private FinalDoorCode _codeToShow;
    [SerializeField] private Texture2D _textureA, _textureB, _textureD, _textureE, _textureF;

    public bool IsPlayingSequence { get; private set; }

    private Texture2D[] _textureSequence;
    private float _desiredNoiseVolume = 0f;

    private void Start()
    {
        var textureSequence = new List<Texture2D>();
        var characters = _codeToShow.Characters;

        foreach (var character in characters)
        {
            var texture = GetTextureFor(character);
            textureSequence.Add(texture);
        }

        _textureSequence = textureSequence.ToArray();

        _material.SetFloat(_noiseMultiplierID, 1f);
        _desiredNoiseVolume = 1f;
    }

    private Texture2D GetTextureFor(CodeCharacter character) => (character) switch
    {
        CodeCharacter.A => _textureA,
        CodeCharacter.B => _textureB,
        CodeCharacter.D => _textureD,
        CodeCharacter.E => _textureE,
        CodeCharacter.F => _textureF,
        _ => throw new System.Exception("Character is not supported")
    };

    private void Update()
    {
        float desiredVolume = _roomTrigger.PlayerInside == true ? _desiredNoiseVolume : 0f;

        if (_staticNoiseSource.volume < desiredVolume)
        {
            _staticNoiseSource.volume += Time.deltaTime;
        }
        else
        {
            _staticNoiseSource.volume -= Time.deltaTime;
        }
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

        _startSound.Play(_startAudioSource);
    }

    private Sequence Show(Texture2D texture)
    {
        return DOTween.Sequence().
            AppendCallback(() => _material.SetTexture(_screenTextureID, texture)).

            Append(_material.DOFloat(_minNoiseValue, _noiseMultiplierID, _transitionDuration)).
            //Join(_staticNoiseSource.DOFade(_minNoiseValue, _transitionDuration)).
            Join(DOTween.To(() => _desiredNoiseVolume, value => _desiredNoiseVolume = value, _minNoiseValue, _transitionDuration)).

            AppendInterval(_displayTime).

            Append(_material.DOFloat(1f, _noiseMultiplierID, _transitionDuration)).
            //Join(_staticNoiseSource.DOFade(1f, _transitionDuration));
            Join(DOTween.To(() => _desiredNoiseVolume, value => _desiredNoiseVolume = value, 1f, _transitionDuration));
    }

}
