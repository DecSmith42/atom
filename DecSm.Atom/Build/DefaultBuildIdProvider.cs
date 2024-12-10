namespace DecSm.Atom.Build;

public sealed class DefaultBuildIdProvider(IBuildVersionProvider buildVersionProvider, IBuildTimestampProvider buildTimestampProvider)
    : IBuildIdProvider
{
    public string BuildId => $"{buildVersionProvider.Version}-{buildTimestampProvider.Timestamp}";

    public string? GetBuildIdGroup(string buildId) =>
        null;
}
