using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISoundEvents : MonoBehaviour
{

    public static event Action<SoundEvent> Event;

    public static void Create(GameObject instigator, Vector3 position, float radius)
    {
        Event?.Invoke(new SoundEvent(instigator, position, radius));
    }

}

public readonly struct SoundEvent
{

    public readonly GameObject Instigator;
    public readonly Vector3 Position;
    public readonly float Radius;
    public readonly float EventTime;

    public SoundEvent(GameObject instigator, Vector3 position, float radius)
    {
        Instigator = instigator;
        Position = position;
        Radius = radius;
        EventTime = Time.time;
    }

}
