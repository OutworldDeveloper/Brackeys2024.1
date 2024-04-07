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

    public override void CreateAttributes(ItemAttributes attributes)
    {
        base.CreateAttributes(attributes);
        attributes.Set(LOADED_AMMO, MaxAmmo);
    }

    public virtual void Shoot(ItemStack stack, Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, 25f, ShootMask) == false)
            return;

        if (hit.transform.TryGetComponent(out Zombie zombie) == true)
        {
            zombie.Kill();
        }
    }

}
