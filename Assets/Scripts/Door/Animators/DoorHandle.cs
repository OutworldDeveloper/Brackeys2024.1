using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class DoorHandle : DoorAnimation
{

    [SerializeField] private float _openAngle = -45f;
    [SerializeField] private float _animationDuration = 0.1f;
    [SerializeField] private Ease _ease = Ease.Linear;

    private Sequence _tween;

    public override void OnEvent(DoorEvent e)
    {
        if (e != DoorEvent.BeginOpening && e != DoorEvent.FailedOpenAttempt)
            return;

        _tween?.Kill();

        _tween = DOTween.Sequence().
            Append(transform.DOLocalRotate(new Vector3(0f, 0f, _openAngle), _animationDuration / 2f).SetEase(_ease)).
            Append(transform.DOLocalRotate(Vector3.zero, _animationDuration / 2f).SetEase(_ease)).
            OnComplete(() => _tween = null);
    }

}
