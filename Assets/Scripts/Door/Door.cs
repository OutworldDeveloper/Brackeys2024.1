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
    public event Action<PlayerCharacter, bool> OpeningAttempt;

    [SerializeField] private Collider _collision;
    [SerializeField] private Transform _rotator;
    [SerializeField] private DoorAnimatorAnimator[] _doorAnimators;
    [SerializeField] private float _animationDuration;
    [SerializeField] private AnimationCurve _openAnimationCurve;
    [SerializeField] private float _openAngle;
    [SerializeField] private ItemTag _keyTag;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Sound _openSound;
    [SerializeField] private Sound _closeSound;
    [SerializeField] private Sound _knockSound;
    [SerializeField] private Sound _lockedSound;
    [SerializeField] private float _openDelay;

    [SerializeField] private BoxCheck _blockersCheck;

    private bool _isOpen;
    private bool _isAnimating;
    private bool _isCollisionSynched;
    private TimeSince _timeSinceAnimationStarted;
    private int _blockedTimes;
    private bool _isLockedByKey;

    private TimeSince _timeSinceLastKnocked = new TimeSince(float.NegativeInfinity);

    public bool IsOpen => _isOpen;
    public bool IsLocked => _isLockedByKey == true || _blockedTimes > 0;

    private void Start()
    {
        _isLockedByKey = _keyTag != null;
    }

    [ContextMenu("Open")]
    public void Open()
    {
        if (_isAnimating == true)
            return;

        if (_isOpen == true)
            return;

        _timeSinceAnimationStarted = new TimeSince(Time.time);
        _isAnimating = true;
        _isCollisionSynched = false;

        _openSound.Play(_audioSource);

        foreach (var animator in _doorAnimators)
        {
            animator.AnimateTo(true, _animationDuration);
        }

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
        _isCollisionSynched = false;

        foreach (var animator in _doorAnimators)
        {
            animator.AnimateTo(false, _animationDuration);
        }

        Closing?.Invoke();
    }

    public bool TryOpen(PlayerCharacter player, bool ignoreLocks = false)
    {
        if (_isOpen == true)
            return false;

        bool success = false;

        if (IsLocked == true && ignoreLocks == false)
        {
            if (player == null)
            {
                Notification.ShowDebug("Tried opening door without player provided");
            }
            else
            {
                if (player.Inventory.TryGetItemWithTag(_keyTag, out Item key) == false)
                {
                    PlayLockedSound();
                    Notification.Show("Locked!");
                }
                else
                {
                    player.Inventory.RemoveAndDestroyItem(key);
                    _isLockedByKey = false;
                    success = true;
                    Notification.Show($"Unlocked");
                }
            }
        }
        else
        {
            success = true;
        }

        if (success == true && _blockersCheck != null && _blockersCheck.Check<Movable>() == true)
        {
            PlayLockedSound();
            Notification.Show($"Blocked!");
            return false;
        }

        OpeningAttempt?.Invoke(player, success);

        if (success == true)
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
        _lockedSound.Play(_audioSource);
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

        if (_isCollisionSynched == false && _timeSinceAnimationStarted > _animationDuration * 0.3f)
        {
            if (_collision != null)
                _collision.enabled = _isOpen;

            _isCollisionSynched = true;
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
                _closeSound.Play(_audioSource);
            }

            if (_isOpen == true)
                Opened?.Invoke();
            else
                Closed?.Invoke();
        }
    }

    private void SetRotation(float angle)
    {
        if (_rotator == null)
            return;

        _rotator.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

}

public abstract class DoorAnimatorAnimator : MonoBehaviour
{
    public abstract void AnimateTo(bool open, float duration);

}
