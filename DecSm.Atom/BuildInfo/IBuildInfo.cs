namespace DecSm.Atom.BuildInfo;

/// <summary>
///     Provides comprehensive build information and configuration parameters for Atom build processes.
/// </summary>
/// <remarks>
///     This interface centralizes build metadata access and provides a consistent way to retrieve
///     build information throughout the build lifecycle.
///     <br /><br />
///     NOTE: Some parameters that change over time (e.g. <see cref="BuildTimestamp" />) should only be calculated
///     once and then cached. This can be done by declaring IBuildInfo parameters as consumed in targets that use the param,
///     so the workflow host can pass the values as workflow variables (see <see cref="ISetupBuildInfo" /> for more).
/// </remarks>
/// <example>
///     <code>
///     public interface IMyBuildTarget : IBuildInfo
///     {
///         Target Build => t => t
///             .ConsumesVariable(nameof(ISetupBuildInfo), nameof(BuildName))
///             .ConsumesVariable(nameof(ISetupBuildInfo), nameof(BuildVersion))
///             .ConsumesVariable(nameof(ISetupBuildInfo), nameof(BuildId))
///             .ConsumesVariable(nameof(ISetupBuildInfo), nameof(BuildTimestamp))
///             .Executes(() =>
///             {
///                 Console.WriteLine($"Building {BuildName} version {BuildVersion}");
///                 Console.WriteLine($"Build ID: {BuildId}");
///                 Console.WriteLine($"Timestamp: {BuildTimestamp}");
///             });
///     }
///     </code>
/// </example>
public interface IBuildInfo : IBuildAccessor
{
    /// <summary>
    ///     Gets the human-readable name of the build.
    /// </summary>
    /// <value>
    ///     A string representing the build name, derived from the solution name or root directory name.
    /// </value>
    /// <remarks>
    ///     Can be configured via the <c>--build-name</c> command-line parameter.
    ///     If not explicitly provided, automatically determined by:
    ///     <list type="number">
    ///         <item>First checking for .sln files in the root directory and using the solution name (without extension)</item>
    ///         <item>Falling back to the root directory name if no solution file is found</item>
    ///     </list>
    /// </remarks>
    [ParamDefinition("build-name", "Name of the build", "{Solution name if provided, otherwise the root directory name}")]
    string BuildName => GetParam(() => BuildName, DefaultBuildName);

    /// <summary>
    ///     Gets the unique identifier for the build/run.
    /// </summary>
    /// <value>
    ///     A string representing the unique build identifier, typically provided by <see cref="IBuildIdProvider" />.
    /// </value>
    /// <remarks>
    ///     Can be configured via the <c>--build-id</c> command-line parameter.
    ///     If not explicitly provided, defaults to the value from the registered <see cref="IBuildIdProvider" />.
    /// </remarks>
    [ParamDefinition("build-id", "Build/run ID", "{From IBuildIdProvider}")]
    string BuildId => GetParam(() => BuildId, DefaultBuildId);

    /// <summary>
    ///     Gets the semantic version of the build.
    /// </summary>
    /// <value>
    ///     A <see cref="SemVer" /> representing the build version following Semantic Versioning specifications.
    /// </value>
    /// <remarks>
    ///     Can be configured via the <c>--build-version</c> command-line parameter.
    ///     If not explicitly provided, defaults to the version from the registered <see cref="IBuildVersionProvider" />.
    ///     The value is automatically converted from string to <see cref="SemVer" /> using the built-in converter.
    /// </remarks>
    [ParamDefinition("build-version", "Build version", "{From IBuildVersionProvider}")]
    SemVer BuildVersion => GetParam(() => BuildVersion, DefaultBuildVersion, BuildVersionConverter);

    /// <summary>
    ///     Gets the build timestamp as Unix time in seconds since epoch (UTC).
    /// </summary>
    /// <value>
    ///     A long integer representing the build timestamp in Unix time format.
    /// </value>
    /// <remarks>
    ///     Can be configured via the <c>--build-timestamp</c> command-line parameter.
    ///     If not explicitly provided, defaults to the timestamp from the registered <see cref="IBuildTimestampProvider" />.
    ///     Represents the moment when the build was initiated or a significant build milestone occurred.
    /// </remarks>
    [ParamDefinition("build-timestamp", "Build timestamp (seconds since unix epoch)", "{From IBuildTimestampProvider}")]
    long BuildTimestamp => GetParam(() => BuildTimestamp, DefaultBuildTimestamp);

    /// <summary>
    ///     Gets the unique identifier for a variation of the build, commonly used for CI/CD matrix jobs.
    /// </summary>
    /// <value>
    ///     An optional string representing the build slice identifier, or <c>null</c> if not specified.
    /// </value>
    /// <remarks>
    ///     Can be configured via the <c>--build-slice</c> command-line parameter.
    ///     This is typically used in CI/CD scenarios where multiple variations of the same build
    ///     are executed in parallel (e.g., different target frameworks, operating systems, or configurations).
    ///     Has no default value and remains <c>null</c> unless explicitly provided.
    /// </remarks>
    [ParamDefinition("build-slice", "Unique identifier for a variation of the build, commonly used for CI/CD matrix jobs")]
    string? BuildSlice => GetParam(() => BuildSlice);

    private string DefaultBuildName
    {
        get
        {
            var solutionFile = FileSystem
                .Directory
                .GetFiles(FileSystem.AtomRootDirectory, "*.sln", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();

            Logger.LogDebug("Determined solution file: {SolutionFile}", solutionFile);

            return solutionFile is not null
                ? new RootedPath(FileSystem, solutionFile).FileName![..^4]
                : FileSystem.AtomRootDirectory.DirectoryName!;
        }
    }

    private string DefaultBuildId =>
        GetService<IBuildIdProvider>()
            .BuildId;

    private SemVer DefaultBuildVersion =>
        GetService<IBuildVersionProvider>()
            .Version;

    private static Func<string?, SemVer?> BuildVersionConverter =>
        s => s is not null
            ? SemVer.Parse(s)
            : throw new ArgumentException("Invalid SemVer");

    private long DefaultBuildTimestamp =>
        GetService<IBuildTimestampProvider>()
            .Timestamp;
}
