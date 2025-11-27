namespace DecSm.Atom.Module.Dotnet;

[PublicAPI]
public sealed record DotnetPackOptions(string ProjectName)
{
    public DotnetPackOptions(RootedPath projectPath) : this(
        projectPath.FileSystem.Path.GetFileNameWithoutExtension(projectPath.Path)) { }

    public bool AutoSetVersion { get; init; } = true;

    public string Configuration { get; init; } = "Release";

    public string? RuntimeIdentifier { get; init; }

    public bool NativeAot { get; init; }

    public string? OutputArtifactName { get; init; }

    public Func<string, string>? CustomPropertiesTransform { get; init; }

    public string? CustomPackageId { get; init; }

    public bool SuppressClearingPublishDirectory { get; init; }
}
