using DG.Tweening;
using UnityEngine;

public sealed class DoorMover : DoorAnimatorAnimator
{

    [SerializeField] private Vector3 _openOffset;
    [SerializeField] private AnimationCurve _animationCurve;

    private Vector3 _originalLocalPosition;

    public override void AnimateTo(bool open, float duration)
    {
        if (open == true)
            _originalLocalPosition = transform.localPosition;

        var targetLocalPosition = open ? _originalLocalPosition + _openOffset : _originalLocalPosition;
        transform.DOLocalMove(targetLocalPosition, duration).SetEase(_animationCurve);
    }

}
