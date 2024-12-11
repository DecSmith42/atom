namespace DecSm.Atom.Build;

[PublicAPI]
public interface IBuildTimestampProvider
{
    long Timestamp { get; }
}
