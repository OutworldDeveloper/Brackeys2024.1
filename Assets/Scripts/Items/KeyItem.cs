using UnityEngine;

[CreateAssetMenu]
public class KeyItem : Item
{
    [field: SerializeField] public Prefab<Transform> KeyModel { get; private set; }

}
