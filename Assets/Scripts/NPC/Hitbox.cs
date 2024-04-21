using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{

    public event Action<Hitbox, float> Damaged;

    [field: SerializeField] public HitboxType HitboxType { get; private set; }

    public void ApplyDamage(float damage)
    {
        Damaged?.Invoke(this, damage);
    }

}

public enum HitboxType
{
    Body,
    Arm,
    Leg,
    Head,
}
