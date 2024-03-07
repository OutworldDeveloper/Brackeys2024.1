using UnityEngine;

public struct TimeSince
{

    public readonly float From;

    public TimeSince(float time)
    {
        From = time;
    }

    public static TimeSince Now() => new TimeSince(Time.time);
    public static TimeSince Never => new TimeSince(float.NegativeInfinity);

    public static implicit operator float(TimeSince timeSince) => UnityEngine.Time.time - timeSince.From;

}

public struct TimeUntil
{

    public readonly float Time;

    public TimeUntil(float time)
    {
        Time = time;
    }

    public static implicit operator float(TimeUntil timeSince) => timeSince.Time - UnityEngine.Time.time;

}
