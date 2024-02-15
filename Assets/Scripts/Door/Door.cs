using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Door : MonoBehaviour
{

    public event Action SomeoneKnocked;
    public event Action Opening;
    public event Action Opened;
    public event Action Closed;
    public event Action Closing;
    public event Action<PlayerCharacter> OpeningAttempt;

    [SerializeField] private Collider _collision;
    [SerializeField] private Transform _rotator;
    [SerializeField] private float _animationDuration;
    [SerializeField] private AnimationCurve _openAnimationCurve;
    [SerializeField] private float _openAngle;
    [SerializeField] private Item _key;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _openSound;
    [SerializeField] private Sound _closeSound;
    [SerializeField] private Sound _knockSound;
    [SerializeField] private Sound _lockedSound;
    [SerializeField] private float _openDelay;

    private bool _isOpen;
    private bool _isAnimating;
    private bool _IsCollisionSynched;
    private TimeSince _timeSinceAnimationStarted;
    private int _blockedTimes;

    private TimeSince _timeSinceLastKnocked = new TimeSince(float.NegativeInfinity);

    public bool IsOpen => _isOpen;
    public bool IsLocked => (IsOpen == false && _key != null) || _blockedTimes > 0;
    public Item Key => _key;

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

        Opening?.Invoke();
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

        Closing?.Invoke();
    }

    public bool TryOpen(PlayerCharacter player)
    {
        if (_isOpen == true)
            return false;

        OpeningAttempt?.Invoke(player);

        if (IsLocked == true)
        {
            if (player.Inventory.HasItem(Key) == false)
            {
                PlayLockedSound();
                Notification.Do("Locked!");
                return false;
            }

            player.Inventory.RemoveItem(Key);
        }

        Delayed.Do(Open, _openDelay);
        return true;
    }

    public void Knock()
    {
        if (_timeSinceLastKnocked < 1f)
            return;

        _timeSinceLastKnocked = new TimeSince(Time.time);

        SomeoneKnocked?.Invoke();
        _knockSound.Play(_audioSource);
    }

    public void PlayLockedSound()
    {
        _lockedSound?.Play(_audioSource);
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
            if (_collision != null)
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

public enum DoorState
{
    Closed,
    Opening,
    Open,
    Closing,
}
