using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Sound")]
public sealed class Sound : ScriptableObject
{

    [SerializeField] private AudioClip[] _clips;
    [SerializeField] private float _pitchMin = 1f, _pitchMax = 1f;
    [SerializeField] private float _volumeMin = 1f, _volumeMax = 1f;
    [SerializeField] private AudioMixerGroup _group;

    public void Play(AudioSource source)
    {
        source.pitch = Random.Range(_pitchMin, _pitchMax);
        source.volume = Random.Range(_volumeMin, _volumeMax);
        var clip = _clips[Random.Range(0, _clips.Length)];
        source.outputAudioMixerGroup = _group;
        source.PlayOneShot(clip);
    }

}
