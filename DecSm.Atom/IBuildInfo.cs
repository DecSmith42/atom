namespace DecSm.Atom;

[TargetDefinition]
public partial interface IBuildInfo
{
    [ParamDefinition("build-id", "Build/run ID")]
    string BuildId => GetParam(() => BuildId)!;

    [ParamDefinition("build-version", "Build version")]
    string BuildVersion => GetParam(() => BuildVersion)!;

    [ParamDefinition("build-timestamp", "Build timestamp (seconds since unix epoch)")]
    string BuildTimestamp => GetParam(() => BuildTimestamp)!;

    [ParamDefinition("build-name", "Name of the build", "{Solution name if provided, otherwise the root directory name}")]
    string AtomBuildName => GetParam(() => AtomBuildName, DefaultBuildName);

    [ParamDefinition("build-slice", "Unique identifier for a variation of the build, commonly used for CI/CD matrix jobs")]
    string? BuildSlice => GetParam(() => BuildSlice);

    private string DefaultBuildName =>
        FileSystem
            .Directory
            .GetFiles(FileSystem.AtomRootDirectory, "*.sln", SearchOption.TopDirectoryOnly)
            .FirstOrDefault() is { } solutionFile
            ? new AbsolutePath(FileSystem, solutionFile).FileName![..^4]
            : FileSystem.AtomRootDirectory.DirectoryName!;
}
