using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WeaponItem : Item
{

    public static readonly ItemAttribute<int> LOADED_AMMO = new ItemAttribute<int>(nameof(LOADED_AMMO));

    [field: Header("Weapon")]
    [field: SerializeField] public Prefab<Weapon> WeaponModel { get; private set; }
    [field: SerializeField] public float Cooldown { get; private set; } = 0.2f;
    [field: SerializeField] public LayerMask ShootMask { get; private set; }
    [field: SerializeField] public Item AmmoItem { get; private set; }
    [field: SerializeField] public int MaxAmmo { get; private set; }
    [field: SerializeField] public float RecoilVerticalMin { get; private set; }
    [field: SerializeField] public float RecoilVerticalMax { get; private set; }
    [field: SerializeField] public float RecoilHorizontalMin { get; private set; }
    [field: SerializeField] public float RecoilHorizontalMax { get; private set; }
    [field: SerializeField] public float CameraShake { get; private set; } = 5f;

    [field: SerializeField] public int BulletsPerShotCount { get; private set; } = 1;
    [field: SerializeField] public MinMax<float> BulletDamage { get; private set; } = new MinMax<float>(1f, 1f);
    [field: SerializeField] public float VerticalBulletSpread { get; private set; } = 1f;
    [field: SerializeField] public float HorizontalBulletSpread { get; private set; } = 1f;

    [field: SerializeField] public Prefab<Transform> HitEffect { get; private set; }
    [field: SerializeField] public Prefab<Transform> HitDecal { get; private set; }

    public override void CreateAttributes(ItemAttributes attributes)
    {
        base.CreateAttributes(attributes);
        attributes.Set(LOADED_AMMO, MaxAmmo);
    }

    public virtual void Shoot(ItemStack stack, Transform from)
    {
        var bulletHits = new List<BulletHit>(BulletsPerShotCount);

        for (int i = 0; i < BulletsPerShotCount; i++)
        {
            Vector2 circlePoint = Random.insideUnitCircle;

            float verticalAngle = circlePoint.y * VerticalBulletSpread;
            float horizontalAngle = circlePoint.x * HorizontalBulletSpread;

            Vector3 bulletDirection =
                Quaternion.AngleAxis(horizontalAngle, from.up) * 
                Quaternion.AngleAxis(verticalAngle, from.right) * 
                from.forward;

            if (Physics.Raycast(from.position, bulletDirection, out RaycastHit hit, 25f, ShootMask) == false)
                continue;

            BulletHit bulletHit = ProcessHit(from.position, bulletDirection, hit);
            bulletHits.Add(bulletHit);

            VisualizeHit(bulletDirection, hit);
        }

        VisualizeHits(bulletHits);
    }

    private BulletHit ProcessHit(Vector3 origin, Vector3 direction, RaycastHit hit)
    {
        if (hit.transform.TryGetComponent(out Hitbox hitbox) == true)
        {
            float damage = Randomize.Float(BulletDamage);
            hitbox.ApplyDamage(damage);
        }

        AISoundEvents.Create(null, origin, 10f);

        return new BulletHit(direction, hit.transform, hit.point, hit.normal, hitbox);
    }

    private void VisualizeHit(Vector3 direction, RaycastHit hit)
    {
        Debug.DrawRay(hit.point, -direction * 0.05f, Color.magenta, 10f);

        if (hit.transform.TryGetComponent(out Surface surface) == false)
            return;

        //Vector3 particleDirection = Vector3.Lerp(hit.normal, -direction, 0.5f);
        //var hitEffect = surface.SurfaceType.BulletHitParticle.Instantiate(hit.point, particleDirection); // -direction
        //Destroy(hitEffect.gameObject, 4f);

        var audioSource = new GameObject().AddComponent<AudioSource>();
        audioSource.transform.position = hit.point;
        audioSource.name = "Temp Audio Source";
        surface.SurfaceType.BulletHitSound.Play(audioSource);
        Destroy(audioSource.gameObject, 4f);
    }

    private void VisualizeHits(List<BulletHit> hits)
    {
        foreach (var hit in hits)
        {
            if (hit.IsHitboxHit == true)
                continue;

            HitDecal.Instantiate(hit.Point - hit.Direction * 0.1f, -hit.Normal).SetParent(hit.Transform, true);
        }

        List<BulletHit> spawned = new List<BulletHit>(hits.Count);

        foreach (var hit in hits)
        {
            if (hit.Transform.TryGetComponent(out Surface surface) == false)
                continue;

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
                var hitEffect = surface.SurfaceType.BulletHitParticle.Instantiate(hit.Point, particleDirection);
                Destroy(hitEffect.gameObject, 4f);
            }
        }

        Debug.Log($"Hits: {hits.Count}, Spawned: {spawned.Count}");
    }

}

public readonly struct BulletHit
{

    public readonly Vector3 Direction;
    public readonly Transform Transform;
    public readonly Vector3 Point;
    public readonly Vector3 Normal;
    public readonly Hitbox Hitbox;

    public BulletHit(Vector3 direction, Transform transform, Vector3 point, Vector3 normal, Hitbox hitbox)
    {
        Direction = direction;
        Transform = transform;
        Point = point;
        Normal = normal;
        Hitbox = hitbox;
    }

    public bool IsHitboxHit => Hitbox != null;

}
