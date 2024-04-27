using Alchemy.Inspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WeaponItem : Item
{

    public static readonly ItemAttribute<int> LOADED_AMMO = new ItemAttribute<int>(nameof(LOADED_AMMO));

    private const string _group = "Weapon";

    [field: SerializeField, FoldoutGroup(_group)] public Prefab<Weapon> WeaponModel { get; private set; }
    [field: SerializeField, FoldoutGroup(_group)] public float Cooldown { get; private set; } = 0.2f;
    [field: SerializeField, FoldoutGroup(_group)] public LayerMask ShootMask { get; private set; }
    [field: SerializeField, FoldoutGroup(_group)] public Item AmmoItem { get; private set; }
    [field: SerializeField, FoldoutGroup(_group)] public int MaxAmmo { get; private set; }
    [field: SerializeField, FoldoutGroup(_group)] public MinMax<float> VerticalRecoil { get; private set; }
    [field: SerializeField, FoldoutGroup(_group)] public MinMax<float> HorizontalRecoil { get; private set; }
    [field: SerializeField, FoldoutGroup(_group)] public float CameraShake { get; private set; } = 5f;
    [field: SerializeField, FoldoutGroup(_group)] public int BulletsPerShotCount { get; private set; } = 1;
    [field: SerializeField, FoldoutGroup(_group)] public MinMax<float> BulletDamage { get; private set; } = new MinMax<float>(1f, 1f);
    [field: SerializeField, FoldoutGroup(_group)] public float VerticalBulletSpread { get; private set; } = 1f;
    [field: SerializeField, FoldoutGroup(_group)] public float HorizontalBulletSpread { get; private set; } = 1f;
    [field: SerializeField, FoldoutGroup(_group)] public HitVisualizer HitVisualizer { get; private set; }

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

            BulletHit bulletHit = ProcessHit(bulletDirection, hit);
            bulletHits.Add(bulletHit);
        }

        HitVisualizer.Visualize(bulletHits.ToArray()); // TODO: Fix
        AISoundEvents.Create(null, from.position, 10f);
    }

    private BulletHit ProcessHit(Vector3 direction, RaycastHit hit)
    {
        if (hit.transform.TryGetComponent(out Hitbox hitbox) == true)
        {
            float damage = Randomize.Float(BulletDamage);
            hitbox.ApplyDamage(damage);
        }

        SurfaceType surfaceType = default;

        if (hit.transform.TryGetComponent(out Surface surface) == true)
        {
            surfaceType = surface.SurfaceType;
        }

        return new BulletHit(direction, hit.transform, hit.point, hit.normal, hitbox, surfaceType);
    }

}

public readonly struct BulletHit
{

    public readonly Vector3 Direction;
    public readonly Transform Transform;
    public readonly Vector3 Point;
    public readonly Vector3 Normal;
    public readonly Hitbox Hitbox;
    public readonly SurfaceType Surface;

    public BulletHit(Vector3 direction, Transform transform, Vector3 point, Vector3 normal, Hitbox hitbox, SurfaceType surface)
    {
        Direction = direction;
        Transform = transform;
        Point = point;
        Normal = normal;
        Hitbox = hitbox;
        Surface = surface;
    }

    public bool IsHitboxHit => Hitbox != null;

}
