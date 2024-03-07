using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimator : MonoBehaviour
{

    [SerializeField] private Door _target;
    [SerializeField] private Animator _animator;

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
            _animator.Play("Open", 0);
    }

}
