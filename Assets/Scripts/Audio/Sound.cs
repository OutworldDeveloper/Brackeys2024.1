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
    [SerializeField] private bool _is3d = true;

    //public void Play(AudioSource source)
    //{
    //    source.pitch = Random.Range(_pitchMin, _pitchMax);
    //    source.volume = Random.Range(_volumeMin, _volumeMax);
    //    var clip = _clips[Random.Range(0, _clips.Length)];
    //    source.outputAudioMixerGroup = _group;
    //    source.PlayOneShot(clip);
    //}

    public void SetupAudioSource(AudioSource source)
    {
        source.pitch = Random.Range(_pitchMin, _pitchMax);
        source.volume = Random.Range(_volumeMin, _volumeMax);
        source.outputAudioMixerGroup = _group;
        source.spatialBlend = _is3d ? 1f : 0f;
    }

    public AudioClip GetRandomClip()
    {
        return _clips[Random.Range(0, _clips.Length)];
    }

}

public static class SoundExtensions
{

    public static void Play(this Sound sound, AudioSource source)
    {
        if (sound == null)
            return;

        sound.SetupAudioSource(source);
        source.PlayOneShot(sound.GetRandomClip());
    }

}
