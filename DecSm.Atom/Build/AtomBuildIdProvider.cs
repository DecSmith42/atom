namespace DecSm.Atom.Build;

internal sealed class AtomBuildIdProvider : IBuildIdProvider
{
    private long? _buildId;

    public long BuildId => _buildId ??= int.Parse($"{(DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch).TotalSeconds:0}");
}