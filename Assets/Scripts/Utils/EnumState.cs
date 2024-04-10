using System;
using System.Collections.Generic;

public sealed class EnumState<TEnum> where TEnum : Enum
{

    public event Action<TEnum> StateChanged;

    private readonly EnumCall<TEnum> _stateChangedCall;
    private readonly Dictionary<TEnum, TimeSince> _timeSinceLastEnter = new Dictionary<TEnum, TimeSince>();

    public TEnum Current { get; private set; }
    public IEnumCall<TEnum> StateStarted => _stateChangedCall;
    public TimeSince TimeSinceLastChange { get; private set; }

    public EnumState()
    {
        _stateChangedCall = new EnumCall<TEnum>(this);
    }

    public void Set(TEnum newState)
    {
        if (Equals(Current, newState) == true)
            return;

        Current = newState;

        TimeSinceLastChange = TimeSince.Now();
        _timeSinceLastEnter.Update(newState, TimeSinceLastChange);

        StateChanged?.Invoke(Current);
        _stateChangedCall.Execute();
    }

    public EnumCall<TEnum> AddCall()
    {
        return new EnumCall<TEnum>(this);
    }

    public TimeSince GetTimeSinceLast(TEnum state)
    {
        if (_timeSinceLastEnter.TryGetValue(state, out TimeSince timeSince) == true)
            return timeSince;

        return TimeSince.Never;
    }

}

public interface IEnumCall<TEnum> where TEnum : Enum
{
    public EnumCall<TEnum> AddCallback(TEnum state, Action action);

}

public sealed class EnumCall<TEnum> : IEnumCall<TEnum> where TEnum : Enum
{

    private readonly EnumState<TEnum> _parent;
    private readonly Dictionary<TEnum, Action> _map = new Dictionary<TEnum, Action>();

    public EnumCall(EnumState<TEnum> parent)
    {
        _parent = parent;
    }

    public void Execute()
    {
        if (_map.TryGetValue(_parent.Current, out Action action) == true)
            action.Invoke();
    }

    public EnumCall<TEnum> AddCallback(TEnum state, Action action)
    {
        _map.Add(state, action);
        return this;
    }

}
