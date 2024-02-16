using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public sealed class AudioManager : MonoBehaviour
{

    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private FloatParameter _soundVolumeParameter;
    [SerializeField] private FloatParameter _musicVolumeParameter;

    private void OnEnable()
    {
        _soundVolumeParameter.Updated += OnSoundVolumeParameterUpdated;
        _musicVolumeParameter.Updated += OnMusicVolumeParameterUpdated;
    }

    private void OnDisable()
    {
        _soundVolumeParameter.Updated -= OnSoundVolumeParameterUpdated;
        _musicVolumeParameter.Updated -= OnMusicVolumeParameterUpdated;
    }

    private void Start()
    {
        OnSoundVolumeParameterUpdated(_soundVolumeParameter.Value);
        OnMusicVolumeParameterUpdated(_musicVolumeParameter.Value);
    }

    private void OnSoundVolumeParameterUpdated(float value)
    {
        _audioMixer.SetFloat("Sounds Volume", Mathf.Log10(value) * 30f);
    }

    private void OnMusicVolumeParameterUpdated(float value)
    {
        _audioMixer.SetFloat("Music Volume", Mathf.Log10(value) * 30f);
    }

}
