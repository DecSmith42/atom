namespace DecSm.Atom.Build;

internal sealed class AtomBuildIdProvider : IBuildIdProvider
{
    private long? _buildId;

    public long BuildId => _buildId ??= long.Parse($"{(DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch).TotalSeconds:0}");
}