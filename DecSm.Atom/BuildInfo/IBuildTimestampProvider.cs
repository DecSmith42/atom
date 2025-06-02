namespace DecSm.Atom.BuildInfo;

/// <summary>
///     Provides the timestamp for the build.
/// </summary>
[PublicAPI]
public interface IBuildTimestampProvider
{
    /// <summary>
    ///     Gets the build timestamp.
    /// </summary>
    /// <remarks>
    ///     Should represent the build timestamp consistently across the build lifecycle.
    ///     Typically provided as Unix time in seconds since epoch (UTC).
    /// </remarks>
    /// <example>
    ///     Example usage:
    ///     <code>
    /// var buildTimestamp = buildTimestampProvider.Timestamp;
    /// // e.g., buildTimestamp = 1704067200 for 2024-01-01T00:00:00Z
    /// </code>
    /// </example>
    long Timestamp { get; }
}
