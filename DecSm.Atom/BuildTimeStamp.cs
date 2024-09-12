namespace DecSm.Atom;

public readonly struct BuildTimeStamp(int value) : IComparable<BuildTimeStamp>,
    IComparisonOperators<BuildTimeStamp, BuildTimeStamp, bool>,
    IEquatable<BuildTimeStamp>
{
    private readonly int _value = value;

    public static BuildTimeStamp Create(DateTimeOffset time) =>
        new((int)((time - DateTimeOffset.UnixEpoch).TotalSeconds / 10.0));

    public static BuildTimeStamp Create(TimeProvider timeProvider) =>
        Create(timeProvider.GetUtcNow());

    public static BuildTimeStamp Create() =>
        Create(TimeProvider.System);

    public DateTimeOffset GetUtc() =>
        DateTimeOffset.UnixEpoch.AddSeconds(_value * 10);

    public override string ToString() =>
        _value.ToString();

    public string ToDateTimeString(IFormatProvider? formatProvider = null) =>
        GetUtc()
            .ToString(formatProvider);

    public int CompareTo(BuildTimeStamp other) =>
        _value.CompareTo(other._value);

    public bool Equals(BuildTimeStamp other) =>
        _value == other._value;

    public override bool Equals(object? obj) =>
        obj is BuildTimeStamp other && Equals(other);

    public override int GetHashCode() =>
        _value;

    public static implicit operator int(BuildTimeStamp buildTimeStamp) =>
        buildTimeStamp._value;

    public static implicit operator BuildTimeStamp(int value) =>
        new(value);

    public static bool operator ==(BuildTimeStamp left, BuildTimeStamp right) =>
        left.Equals(right);

    public static bool operator !=(BuildTimeStamp left, BuildTimeStamp right) =>
        !left.Equals(right);

    public static bool operator >(BuildTimeStamp left, BuildTimeStamp right) =>
        left.CompareTo(right) > 0;

    public static bool operator >=(BuildTimeStamp left, BuildTimeStamp right) =>
        left.CompareTo(right) >= 0;

    public static bool operator <(BuildTimeStamp left, BuildTimeStamp right) =>
        left.CompareTo(right) < 0;

    public static bool operator <=(BuildTimeStamp left, BuildTimeStamp right) =>
        left.CompareTo(right) <= 0;
}
