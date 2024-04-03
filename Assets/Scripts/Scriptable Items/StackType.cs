using UnityEngine;

[CreateAssetMenu(menuName = "Stack Type")]
public sealed class StackType : ScriptableObject
{
    [field: SerializeField] public int MaxCount { get; private set; }

}
