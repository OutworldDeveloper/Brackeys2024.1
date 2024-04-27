using System.Collections.Generic;
using UnityEngine;
using Alchemy.Serialization;
using System;

[CreateAssetMenu]
[AlchemySerialize]
public partial class HitVisualizer : ScriptableObject
{

    [AlchemySerializeField, NonSerialized]
    private Dictionary<SurfaceType, Prefab<Transform>> _effects;

    [SerializeField] private Prefab<Transform> _defaultEffect;

    [AlchemySerializeField, NonSerialized]
    private Dictionary<SurfaceType, Prefab<Transform>> _decals;

    [SerializeField] private Prefab<Transform> _defaultDecal;

    [SerializeField] private Sound _defaultHitboxSound;

    public virtual void Visualize(BulletHit[] hits)
    {
        VisualizeDecals(hits);
        VisualizeEffects(hits);
        PlaySound(hits);
    }

    protected virtual void VisualizeDecals(BulletHit[] hits)
    {
        foreach (var hit in hits)
        {
            if (hit.IsHitboxHit == true)
                continue;
        
            _decals.Resolve(hit.Surface, _defaultDecal).
                Instantiate(hit.Point - hit.Direction * 0.1f, -hit.Normal).
                SetParent(hit.Transform, true);
        }
    }

    protected virtual void VisualizeEffects(BulletHit[] hits)
    {
        List<BulletHit> spawned = new List<BulletHit>(hits.Length);

        foreach (var hit in hits)
        {
            float minDistance = Mathf.Infinity;

            foreach (var spawnedHit in spawned)
            {
                float distance = Vector3.Distance(spawnedHit.Point, hit.Point);

                if (distance < minDistance)
                    minDistance = distance;
            }

            if (minDistance > 0.2f)
            {
                spawned.Add(hit);
                Vector3 particleDirection = Vector3.Lerp(hit.Normal, -hit.Direction, 0.5f);
                var hitEffect = _effects.Resolve(hit.Surface, _defaultEffect).
                    Instantiate(hit.Point, particleDirection);
                Destroy(hitEffect.gameObject, 4f);
            }
        }

        Debug.Log($"Hits: {hits.Length}, Spawned: {spawned.Count}");
    }

    protected virtual void PlaySound(BulletHit[] hits)
    {
        foreach (var hit in hits)
        {
            if (hit.IsHitboxHit == false)
                continue;

            var audioSource = new GameObject().AddComponent<AudioSource>();
            audioSource.transform.position = hit.Point;
            audioSource.name = "Temp Audio Source";
            _defaultHitboxSound.Play(audioSource);
            Destroy(audioSource.gameObject, 4f);
            break;
        }
    }

}
