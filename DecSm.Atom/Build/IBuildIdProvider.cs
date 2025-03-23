namespace DecSm.Atom.Build;

/// <summary>
///     Provides a build ID.
/// </summary>
public interface IBuildIdProvider
{
    /// <summary>
    ///     The build ID.
    /// </summary>
    /// <remarks>
    ///     Should be consistent within a build, but unique across builds.
    ///     If the build ID is not consistent within multiple jobs within a build, the build ID should be
    ///     passed as a variable between jobs.
    /// </remarks>
    string BuildId { get; }

    /// <summary>
    ///     Optional representation for grouping build IDs.
    ///     This can be used by other systems e.g. artifact storage to group build IDs in folders.
    /// </summary>
    /// <param name="buildId">The existing build ID.</param>
    /// <returns>The group string or null if no grouping is implemented.</returns>
    /// <example>
    ///     If the build ID is date-based, we could group by month
    ///     <code>
    /// var group = GetBuildIdGroup("2021-01-01-123456");
    /// // group = "2021-01"
    /// </code>
    ///     If the build ID is a SemVer, we could group by major.minor.patch
    ///     <code>
    /// var group = GetBuildIdGroup("1.2.3");
    /// // group = "1.2.3"
    /// </code>
    /// </example>
    string? GetBuildIdGroup(string buildId) =>
        null;
}
