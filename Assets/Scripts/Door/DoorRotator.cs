using DG.Tweening;
using UnityEngine;

public sealed class DoorRotator : DoorAnimatorAnimator
{

    [SerializeField] private RotationType _rotationType;
    [SerializeField] private float _openedAngle;
    [SerializeField] private AnimationCurve _animationCurve;

    public override void AnimateTo(bool open, float duration)
    {
        var angle = open ? _openedAngle : 0f;
        var localEuler = 
            _rotationType == RotationType.X ?
                new Vector3(angle, 0f, 0f) :
            _rotationType == RotationType.Y ?
                new Vector3(0f, angle, 0f) :
                new Vector3(0f, 0f, angle);

        transform.DOLocalRotate(localEuler, duration, RotateMode.Fast).SetEase(_animationCurve);
    }

    private enum RotationType
    {
        X,
        Y,
        Z,
    }

}
