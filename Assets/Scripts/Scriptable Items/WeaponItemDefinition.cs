using UnityEngine;

[CreateAssetMenu]
public class WeaponItemDefinition : ItemDefinition
{

    [field: Header("Weapon")]
    [field: SerializeField] public Prefab<GameObject> WeaponModel { get; private set; }

}
