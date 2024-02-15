using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandle : MonoBehaviour
{

    [SerializeField] private Door _door;
    [SerializeField] private float _openAngle = -45f;
    [SerializeField] private float _animationDuration = 0.1f;
    [SerializeField] private Ease _ease = Ease.Linear;

    private Sequence _tween;

    private void OnEnable()
    {
        _door.Opening += OnDoorOpening;
    }

    private void OnDisable()
    {
        _door.Opening -= OnDoorOpening;
    }

    private void OnDoorOpening()
    {
        _tween?.Kill();

        _tween = DOTween.Sequence().
            Append(transform.DOLocalRotate(new Vector3(0f, 0f, _openAngle), _animationDuration / 2f).SetEase(_ease)).
            Append(transform.DOLocalRotate(Vector3.zero, _animationDuration / 2f).SetEase(_ease)).
            OnComplete(() => _tween = null);
    }

}
