using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Code")]
public sealed class Code : ScriptableObject
{
    [field: SerializeField] public string Value { get; private set; }

}
