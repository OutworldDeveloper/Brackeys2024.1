using System;
using UnityEngine;

public sealed class PawnAction
{

    public event Action StateChanged;

    public readonly string Description;
    public readonly KeyCode Key;
    public readonly KeyCode[] AdditionalKeys;

    public PawnAction(string description, KeyCode key, params KeyCode[] additionalKeys)
    {
        Description = description;
        Key = key;
        AdditionalKeys = additionalKeys;
    }

    public bool IsActive { get; private set; }

    public void Enable()
    {
        IsActive = true;
        StateChanged?.Invoke();
    }

    public void Disable()
    {
        IsActive = false;
        StateChanged?.Invoke();
    }

}
