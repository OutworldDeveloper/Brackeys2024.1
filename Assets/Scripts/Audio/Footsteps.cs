using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Footsteps : MonoBehaviour
{

    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private Sound _sound;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _cooldown = 0.2f;

    private TimeSince _timeSinceLastStep;
    private Vector3 _lastStepPosition;

    private void Update()
    {
        if (_player.IsDead == true)
            return;

        //if (_timeSinceLastStep < _cooldown)
        //    return;

        if (_player.HorizontalVelocity.magnitude < 0.05f)
            return;

        if (_player.IsGrounded == false)
            return;

        if (Vector3.Distance(_lastStepPosition, _player.transform.position) < 1.3f)
            return;

        _sound.Play(_audioSource);
        _timeSinceLastStep = new TimeSince(Time.time);
        _lastStepPosition = _player.transform.position;
    }

}
