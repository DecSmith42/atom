namespace DecSm.Atom.Build;

/// <summary>
///     Default Build ID provider, used if no custom implementation is provided.
///     The build ID is composed of the build version and the build timestamp.
/// </summary>
public sealed class DefaultBuildIdProvider(IBuildVersionProvider buildVersionProvider, IBuildTimestampProvider buildTimestampProvider)
    : IBuildIdProvider
{
    /// <summary>
    ///     Gets the build ID.
    /// </summary>
    /// <example>1.0.0-20220101T000000</example>
    public string BuildId => $"{buildVersionProvider.Version}-{buildTimestampProvider.Timestamp}";

    /// <summary>
    ///     Gets the build ID group. Unused in the default implementation.
    /// </summary>
    /// <returns>null</returns>
    public string? GetBuildIdGroup(string buildId) =>
        null;
}
