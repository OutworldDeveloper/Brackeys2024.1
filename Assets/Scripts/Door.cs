using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Door : MonoBehaviour
{

    public event Action SomeoneKnocked;
    public event Action Opened;
    public event Action Closed;

    [SerializeField] private Collider _collision;
    [SerializeField] private Transform _rotator;
    [SerializeField] private float _animationDuration;
    [SerializeField] private AnimationCurve _openAnimationCurve;
    [SerializeField] private float _openAngle;
    [SerializeField] private ItemTag _keyTag;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _openSound;
    [SerializeField] private Sound _closeSound;
    [SerializeField] private Sound _knockSound;

    private bool _isOpen;
    private bool _isAnimating;
    private bool _IsCollisionSynched;
    private TimeSince _timeSinceAnimationStarted;
    private int _blockedTimes;

    public bool IsOpen => _isOpen;
    public bool IsLocked => (IsOpen == false && _keyTag != null) || _blockedTimes > 0;
    public ItemTag KeyTag => _keyTag;

    [ContextMenu("Open")]
    public void Open()
    {
        if (_isAnimating == true)
            return;

        if (_isOpen == true)
            return;

        _timeSinceAnimationStarted = new TimeSince(Time.time);
        _isAnimating = true;
        _IsCollisionSynched = false;

        _openSound?.Play(_audioSource);
    }

    [ContextMenu("Close")]
    public void Close()
    {
        if (_isAnimating == true)
            return;

        if (_isOpen == false)
            return;

        _timeSinceAnimationStarted = new TimeSince(Time.time);
        _isAnimating = true;
        _IsCollisionSynched = false;
    }

    public void Knock()
    {
        SomeoneKnocked?.Invoke();
        _knockSound.Play(_audioSource);
    }

    public void Block()
    {
        _blockedTimes++;
    }

    public void Unblock()
    {
        _blockedTimes--;
    }

    private void Update()
    {
        if (_isAnimating == false)
            return;

        if (_IsCollisionSynched == false && _timeSinceAnimationStarted > _animationDuration * 0.3f)
        {
            _collision.enabled = _isOpen;
            _IsCollisionSynched = true;
        }

        float startAngle = _isOpen ? _openAngle : 0f;
        float targetAngle = _isOpen ? 0f : _openAngle;

        if (_timeSinceAnimationStarted < _animationDuration)
        {
            float t = _timeSinceAnimationStarted / _animationDuration;
            float curvedT = _openAnimationCurve.Evaluate(t);
            float angle = Mathf.Lerp(startAngle, targetAngle, curvedT);
            SetRotation(angle);
        }
        else
        {
            SetRotation(targetAngle);
            _isOpen = !_isOpen;
            _isAnimating = false;

            if (_isOpen == false)
            {
                _closeSound?.Play(_audioSource);
            }

            if (_isOpen == true)
                Opened?.Invoke();
            else
                Closed?.Invoke();
        }
    }

    private void SetRotation(float angle)
    {
        _rotator.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

}
