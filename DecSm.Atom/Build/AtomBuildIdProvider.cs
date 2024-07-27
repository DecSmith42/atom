namespace DecSm.Atom.Build;

internal sealed class AtomBuildIdProvider : IBuildIdProvider
{
    private string? _buildId;

    public string BuildId => _buildId ??= $"{(DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch).TotalSeconds:0}";
}