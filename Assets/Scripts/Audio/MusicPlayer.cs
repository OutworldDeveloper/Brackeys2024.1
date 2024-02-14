using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class MusicPlayer : MonoBehaviour
{

    [SerializeField] private AudioClip[] _music;
    [SerializeField] private float _delayTimeMin = 10f, _delayTimeMax = 15f;

    private TimeUntil _timeUntilNextMusic;
    private int _nextMusicIndex;

    private void Start()
    {
        _timeUntilNextMusic = new TimeUntil(Time.time + Random.Range(_delayTimeMin, _delayTimeMax));
    }

    private void Update()
    {
        if (_timeUntilNextMusic > 0f)
            return;

        AudioClip toPlay = _music[_nextMusicIndex];
        var audioSource = GetComponent<AudioSource>();
        audioSource.clip = toPlay;
        audioSource.Play();
        _timeUntilNextMusic = new TimeUntil(Time.time + toPlay.length + Random.Range(_delayTimeMin, _delayTimeMax));
        _nextMusicIndex++;
        if (_nextMusicIndex >= _music.Length)
            _nextMusicIndex = 0;
    }

}
