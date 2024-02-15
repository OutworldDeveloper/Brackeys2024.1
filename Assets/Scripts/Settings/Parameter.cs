using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Parameter<T> : ScriptableObject where T : struct
{

    public event Action<T> Updated;

    [SerializeField] private T defaultValue;

    private T _cashedValue;
    private bool _hasCashedValue;

    public T Value
    {
        get
        {
            if (_hasCashedValue)
                return _cashedValue;

            if (PlayerPrefs.HasKey(name))
            {
                _cashedValue = LoadValue();
                _hasCashedValue = true;
                return _cashedValue;
            }

            return defaultValue;
        }
    }

    public void SetValue(T value)
    {
        _hasCashedValue = false;
        SaveValue(value);
        OnValueChanged();
        Updated?.Invoke(value);
    }

    public void ResetValue() => SetValue(defaultValue);

    protected abstract T LoadValue();
    protected abstract void SaveValue(T value);
    protected virtual void OnValueChanged() { }

}