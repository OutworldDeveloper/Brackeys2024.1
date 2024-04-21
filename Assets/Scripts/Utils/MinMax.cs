using System;
using UnityEngine;

[Serializable]
public sealed class MinMax<T> where T : struct
{
    [field: SerializeField] public T Min { get; private set; }
    [field: SerializeField] public T Max { get; private set; }

    public MinMax(T min, T max)
    {
        Min = min;
        Max = max;
    }

}
