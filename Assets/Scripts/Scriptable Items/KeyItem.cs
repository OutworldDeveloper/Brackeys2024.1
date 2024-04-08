using UnityEngine;

[CreateAssetMenu]
public class KeyItem : Item
{
    [field: SerializeField] public Prefab<GameObject> KeyModel { get; private set; }

}
