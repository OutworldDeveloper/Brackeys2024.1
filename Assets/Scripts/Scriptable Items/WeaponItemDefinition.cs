using UnityEngine;

[CreateAssetMenu]
public class WeaponItemDefinition : ItemDefinition
{

    [field: Header("Weapon")]
    [field: SerializeField] public Prefab<Weapon> WeaponModel { get; private set; }
    [field: SerializeField] public float Cooldown { get; private set; } = 0.2f;
    [field: SerializeField] public LayerMask ShootMask { get; private set; }

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
