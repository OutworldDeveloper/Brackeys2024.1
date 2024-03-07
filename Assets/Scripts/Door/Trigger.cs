using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trigger<T> : MonoBehaviour where T : Component
{

    [field: SerializeField] public TEvent EnterEvent { get; private set; }
    [field: SerializeField] public TEvent ExitEvent { get; private set; }
    [field: SerializeField] public TEvent StayEvent { get; private set; }

    [SerializeField] private bool _useStayEvent;
    [SerializeField] private float _stayTimeRequired = 0.75f;

    private bool _stayEventSent;
    private TimeSince _timeSinceLastEntered = new TimeSince(float.NegativeInfinity);
    private readonly List<T> _targetsInside = new List<T>();

    public bool IsEverVisited { get; private set; }
    public bool HasTargetInside => _targetsInside.Count > 0;
    public T TargetInside => _targetsInside[0];

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out T target) == true)
        {
            _targetsInside.Add(target);
            IsEverVisited = true;
            EnterEvent.Invoke(target);
            _timeSinceLastEntered = new TimeSince(Time.time);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out T target) == true)
        {
            _targetsInside.Remove(target);
            ExitEvent.Invoke(target);
            _stayEventSent = false;
        }
    }

    private void Update()
    {
        if (_useStayEvent == false || HasTargetInside == false || _stayEventSent == true)
            return;

        if (_timeSinceLastEntered > _stayTimeRequired)
        {
            StayEvent.Invoke(TargetInside);
            _stayEventSent = true;
        }
    }

    [Serializable]
    public sealed class TEvent : UnityEngine.Events.UnityEvent<T> { }

}