using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterDoorValve : DoorAnimation
{

    [SerializeField] private Animator _animator;

    public override void OnEvent(DoorEvent e)
    {
        if (e != DoorEvent.BeginOpening)
            return;

        _animator.Play("Open", 0);
    }

}
