using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SurfaceType : ScriptableObject
{
    [field: SerializeField] public Prefab<ParticleSystem> BulletHitParticle { get; private set; }


}
