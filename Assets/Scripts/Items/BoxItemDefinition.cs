using UnityEngine;

[CreateAssetMenu]
public class BoxItemDefinition : Item
{

    [field: Header("Box")]
    [field: SerializeField] public Item Reward { get; private set; }

}
