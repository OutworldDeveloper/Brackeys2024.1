using System;
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

        Vector3 pointA = soundEvent.Position;
        Vector3 pointB = transform.position + Vector3.up;
        LayerMask layerMask = LayerMask.NameToLayer("Default");

        if (Physics.Linecast(pointA, pointB) == true)
        {
            Debug.DrawLine(pointA, pointB, Color.red, 5f);
            return;
        }

        LastEvent = soundEvent;
        Perceived?.Invoke(soundEvent);

        Debug.DrawLine(pointA, pointB, Color.blue, 5f);
    }

}
