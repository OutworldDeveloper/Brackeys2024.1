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

    public override void CreateAttributes(ItemAttributes attributes)
    {
        base.CreateAttributes(attributes);
        attributes.Set(LOADED_AMMO, MaxAmmo);
    }

    public virtual void Shoot(ItemStack stack, Transform from)
    {
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

            ProcessHit(bulletDirection, hit);
            VisualizeHit(bulletDirection, hit);
        }
    }

    private void ProcessHit(Vector3 direction, RaycastHit hit)
    {
        if (hit.transform.TryGetComponent(out Hitbox hitbox) == false)
            return;

        float damage = Randomize.Float(BulletDamage);
        hitbox.ApplyDamage(damage);
    }

    private void VisualizeHit(Vector3 direction, RaycastHit hit)
    {
        Debug.DrawRay(hit.point, -direction * 0.05f, Color.magenta, 10f);

        if (hit.transform.TryGetComponent(out Surface surface) == false)
            return;

        Vector3 particleDirection = Vector3.Lerp(hit.normal, -direction, 0.5f);
        var hitEffect = surface.SurfaceType.BulletHitParticle.Instantiate(hit.point, particleDirection); // -direction
        Destroy(hitEffect.gameObject, 4f);
    }

}
