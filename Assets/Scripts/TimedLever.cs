using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public sealed class TimedLever : MonoBehaviour
{

    [SerializeField] private Transform _rotator;
    [SerializeField] private float _stayingActiveDuration = 4f;
    [SerializeField] private Sound _activatedSound;
    [SerializeField] private Sound _deactivatedSound;
    [SerializeField] private Sound _tickSound;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private UnityEvent _activated;
    [SerializeField] private UnityEvent _deactivated;

    public bool IsActivated { get; private set; }
    public TimeUntil _timeUntilDeactivation;
    public TimeSince _timeSinceLastTickSound;

    private Sequence _activeSequence;

    public void Activate()
    {
        if (IsActivated == true)
        {
            Debug.LogError("Cannot activate activated lever", this);
            return;
        }

        IsActivated = true;
        _timeUntilDeactivation = new TimeUntil(Time.time + _stayingActiveDuration);
        OnActivated();
    }

    private void Update()
    {
        if (IsActivated == false)
            return;

        if (_timeUntilDeactivation < 0)
        {
            IsActivated = false;
            OnDeactivated();
            return;
        }

        if (_timeSinceLastTickSound > 1)
        {
            _tickSound.Play(_audioSource);
            _timeSinceLastTickSound = new TimeSince(Time.time);
        }
    }

    private void OnActivated()
    {
        _timeSinceLastTickSound = new TimeSince(Time.time);

        _activeSequence?.Kill(true);
        _activeSequence = DOTween.Sequence().
            Append(_rotator.DOLocalRotate(new Vector3(180f, 0f, 0f), 0.2f)).
            Append(_rotator.DOLocalRotate(new Vector3(0f, 0f, 0f), _stayingActiveDuration)).
            OnComplete(() => _activeSequence = null);

        _activatedSound.Play(_audioSource);
        _activated.Invoke();
    }

    private void OnDeactivated()
    {
        _deactivatedSound.Play(_audioSource);
        _deactivated.Invoke();
    }

}
