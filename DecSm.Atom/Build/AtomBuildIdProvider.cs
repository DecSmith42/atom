namespace DecSm.Atom.Build;

internal sealed class AtomBuildIdProvider : IBuildIdProvider
{
    private string? _buildId;

    public string BuildId => _buildId ??= $"{(DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch).TotalSeconds:0}";

    public string GetBuildIdPathPrefix(string buildId)
    {
        if (!long.TryParse(buildId, out var unixSeconds))
            throw new InvalidOperationException($"Failed to parse build ID '{buildId}' as Unix seconds");

        var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

        return $"{dateTime.Year}/{dateTime.Month}";
    }
}
