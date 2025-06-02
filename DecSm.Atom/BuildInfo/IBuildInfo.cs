namespace DecSm.Atom.BuildInfo;

public interface IBuildInfo : IBuildAccessor
{
    [ParamDefinition("build-id", "Build/run ID", "{From IBuildIdProvider}")]
    string BuildId => GetParam(() => BuildId, DefaultBuildId);

    [ParamDefinition("build-version", "Build version", "{From IBuildVersionProvider}")]
    SemVer BuildVersion => GetParam(() => BuildVersion, DefaultBuildVersion, BuildVersionConverter);

    [ParamDefinition("build-timestamp", "Build timestamp (seconds since unix epoch)", "{From IBuildTimestampProvider}")]
    long BuildTimestamp => GetParam(() => BuildTimestamp, DefaultBuildTimestamp);

    [ParamDefinition("build-name", "Name of the build", "{Solution name if provided, otherwise the root directory name}")]
    string AtomBuildName => GetParam(() => AtomBuildName, DefaultBuildName);

    [ParamDefinition("build-slice", "Unique identifier for a variation of the build, commonly used for CI/CD matrix jobs")]
    string? BuildSlice => GetParam(() => BuildSlice);

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
}
