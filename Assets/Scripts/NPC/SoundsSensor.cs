using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsSensor : MonoBehaviour
{

    public event Action<SoundEvent> Perceived;

    public SoundEvent LastEvent { get; private set; }

    private void OnEnable()
    {
        AISoundEvents.Event += OnSoundEvent;    
    }

    private void OnDisable()
    {
        AISoundEvents.Event -= OnSoundEvent;
    }

    private void OnSoundEvent(SoundEvent soundEvent)
    {
        float eventDistance = Vector3.Distance(soundEvent.Position, transform.position);

        if (eventDistance > soundEvent.Radius)
            return;

        LastEvent = soundEvent;
        Perceived?.Invoke(soundEvent);
    }

}
