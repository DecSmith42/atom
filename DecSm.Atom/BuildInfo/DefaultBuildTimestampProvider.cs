namespace DecSm.Atom.BuildInfo;

/// <summary>
///     Default implementation of <see cref="IBuildTimestampProvider" /> that provides Unix timestamps for build processes.
/// </summary>
/// <remarks>
///     This provider generates a build timestamp based on the current UTC time when first accessed.
///     The timestamp is cached after the first call to ensure consistency throughout the build process.
///     Uses the injected <see cref="TimeProvider" /> to obtain the current time, making it testable and mockable.
/// </remarks>
/// <param name="timeProvider">The time provider used to get the current UTC time.</param>
internal sealed class DefaultBuildTimestampProvider(TimeProvider timeProvider) : IBuildTimestampProvider
{
    /// <summary>
    ///     Cached build timestamp value to ensure consistency across multiple accesses.
    /// </summary>
    private long? _buildTimestamp;

    /// <summary>
    ///     Gets the build timestamp as Unix seconds (seconds since January 1, 1970 UTC).
    /// </summary>
    /// <value>
    ///     A Unix timestamp representing when the build timestamp was first requested.
    ///     The value is cached on first access to ensure consistency within the process.
    ///     (Note: The value is not cached between processes e.g. in different workflow jobs.
    ///     See <see cref="TargetDefinition.ConsumesVariable" /> for more)
    /// </value>
    public long Timestamp =>
        _buildTimestamp ??= timeProvider
            .GetUtcNow()
            .ToUnixTimeSeconds();
}
