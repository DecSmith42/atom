namespace DecSm.Atom.Build;

public sealed class AtomBuildIdProvider : IBuildIdProvider
{
    private long? _buildId;

    public long BuildId => _buildId ??= int.Parse($"{(DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch).Seconds}");
}