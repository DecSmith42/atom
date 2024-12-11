namespace DecSm.Atom.Build;

internal sealed class DefaultBuildTimestampProvider(TimeProvider timeProvider) : IBuildTimestampProvider
{
    private long? _buildTimestamp;

    public long Timestamp =>
        _buildTimestamp ??= timeProvider
            .GetUtcNow()
            .ToUnixTimeSeconds();
}
