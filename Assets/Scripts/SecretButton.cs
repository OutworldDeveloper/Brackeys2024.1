using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SecretButton : MonoBehaviour
{

    [SerializeField] private UnityEvent _pressedEvent;
    [SerializeField] private float _cooldown = 0.2f;

    private TimeSince _timeSinceLastPress;

    public void Press()
    {
        if (_timeSinceLastPress < _cooldown)
            return;

        _timeSinceLastPress = new TimeSince(Time.time);
        _pressedEvent?.Invoke();
    }
    
}
