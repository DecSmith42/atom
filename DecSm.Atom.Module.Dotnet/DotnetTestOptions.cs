namespace DecSm.Atom.Module.Dotnet;

[PublicAPI]
public sealed record DotnetTestOptions(string ProjectName)
{
    public bool AutoSetVersion { get; init; } = true;

    public string? Configuration { get; init; }

    public string? Framework { get; init; }

    public string? OutputArtifactName { get; init; }

    public Func<string, string>? CustomPropertiesTransform { get; init; }
}
