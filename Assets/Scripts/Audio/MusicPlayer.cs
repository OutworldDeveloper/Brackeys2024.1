using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class MusicPlayer : MonoBehaviour
{

    [SerializeField] private AudioClip _kitchenEnterMusic;
    [SerializeField] private AudioClip _victoryMusic;
    [SerializeField] private AudioClip[] _music;
    [SerializeField] private float _delayTimeMin = 10f, _delayTimeMax = 15f;
    [SerializeField] private float _musicFadeOutSpeed = 0.1f;

    [SerializeField] private PlayerTrigger _kitchenTrigger;
    [SerializeField] private Ghost _ghost;
    [SerializeField] private Door _finalDoor;

    private AudioSource _audioSource;

    private TimeUntil _timeUntilNextMusic;
    private int _nextMusicIndex;

    private AudioClip _toPlay;
    private bool _visitedKitchen;

    private MusicState _previousState;
    private MusicState _state;
    private TimeSince _timeSinceLastStateChange;

    private float _maxVolume;

    private float _ghostNoChaseTime;

    private void OnEnable()
    {
        _audioSource = GetComponent<AudioSource>();
        _kitchenTrigger.EnterEvent.AddListener(OnVisitedKitchen);
        _finalDoor.Opened += OnFinalDoorOpened;

        _maxVolume = _audioSource.volume;
    }

    private void OnDisable()
    {
        _kitchenTrigger.EnterEvent.RemoveListener(OnVisitedKitchen);
        _finalDoor.Opened -= OnFinalDoorOpened;
    }

    private void Start()
    {
        _timeUntilNextMusic = new TimeUntil(Time.time + Random.Range(_delayTimeMin, _delayTimeMax));
    }

    private void Update()
    {
        HandleState();

        if (_audioSource.clip == _toPlay)
        {
            _audioSource.volume = _maxVolume;
            return;
        }

        if (_audioSource.isPlaying == true) // If we're playing, but shouldn't, turn the volume down
        {
            if (_audioSource.volume > 0.001f)
            {
                _audioSource.volume -= Time.deltaTime * _musicFadeOutSpeed;
            }
            else
            {
                _audioSource.Stop();
                _audioSource.clip = null;
            }
        }
        else
        {
            _audioSource.clip = _toPlay;
            _audioSource.volume = _maxVolume;

            if (_audioSource.clip != null)
                _audioSource.Play();
        }
    }

    private void SetState(MusicState state)
    {
        if (_state == state)
            return;

        _previousState = _state;
        _state = state;
        _timeSinceLastStateChange = new TimeSince(Time.time);
        _toPlay = null;

        if (Application.isEditor == true)
            Notification.Show($"{_previousState} => {_state}");

        switch (state)
        {
            case MusicState.ExploringKitchen:
                _toPlay = _kitchenEnterMusic;
                break;

            case MusicState.Gameplay:
                _timeUntilNextMusic = new TimeUntil(Time.time + GetRandomDelay());
                break;

            case MusicState.FinishLine:
                _toPlay = _victoryMusic;
                break;
        }
    }

    private void HandleState()
    {
        if (_ghost.State == GhostState.Chasing && _state != MusicState.FinishLine)
        {
            SetState(MusicState.GhostChase);
            return;
        }

        switch (_state)
        {
            case MusicState.ExploringKitchen:
                if (_timeSinceLastStateChange > _kitchenEnterMusic.length + 2f)
                    SetState(MusicState.Gameplay);
                break;

            case MusicState.Gameplay:
                if (_timeUntilNextMusic > 0f)
                    break;

                _toPlay = _music[_nextMusicIndex];
                _timeUntilNextMusic = new TimeUntil(Time.time + _toPlay.length + Random.Range(_delayTimeMin, _delayTimeMax));
                _nextMusicIndex++;
                if (_nextMusicIndex >= _music.Length)
                    _nextMusicIndex = 0;
                break;

            case MusicState.GhostChase:

                if (_ghost.State == GhostState.Chasing)
                {
                    _ghostNoChaseTime = 0f;
                }
                else
                {
                    _ghostNoChaseTime += Time.deltaTime;
                    if (_ghostNoChaseTime > 5f)
                    {
                        SetState(_previousState);
                    } 
                }
                break;

            case MusicState.FinishLine:
                break;
        }
    }

    private void OnVisitedKitchen(PlayerCharacter player)
    {
        if (_visitedKitchen == true)
            return;

        SetState(MusicState.ExploringKitchen);
        _visitedKitchen = true;
    }

    private void OnFinalDoorOpened()
    {
        SetState(MusicState.FinishLine);
    }

    private float GetRandomDelay()
    {
        return Random.Range(_delayTimeMin, _delayTimeMax);
    }

    private enum MusicState
    {
        None,
        ExploringKitchen,
        Gameplay,
        GhostChase,
        FinishLine,
    }

}
