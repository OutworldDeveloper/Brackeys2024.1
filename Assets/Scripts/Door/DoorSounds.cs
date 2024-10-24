using UnityEngine;

public sealed class DoorSounds : MonoBehaviour
{

    [SerializeField] private Door _target;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _openAttemptSuccessSound;
    [SerializeField] private Sound _openAttemptFailSound;

    private void OnEnable()
    {
        _target.OpeningAttempt += OnDoorOpening;
    }

    private void OnDisable()
    {
        _target.OpeningAttempt -= OnDoorOpening;
    }

    private void OnDoorOpening(PlayerCharacter player, bool success)
    {
        if (success == true)
            _openAttemptSuccessSound.Play(_audioSource);
        else
            _openAttemptFailSound.Play(_audioSource);
    }

}
