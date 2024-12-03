namespace DecSm.Atom.Module.Dotnet;

public sealed record DotnetPublishOptions(string ProjectName)
{
    public bool AutoSetVersion { get; init; } = true;

    public string Configuration { get; init; } = "Release";

    public string? OutputArtifactName { get; init; }
}
