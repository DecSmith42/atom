namespace DecSm.Atom.Artifacts;

[PublicAPI]
public interface IArtifactProvider
{
    IReadOnlyList<string> RequiredParams => [];

    Task UploadArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null);

    Task DownloadArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null);

    Task DownloadArtifact(string artifactName, IReadOnlyList<string> buildIds);

    Task Cleanup(IReadOnlyList<string> buildIds);

    Task<IReadOnlyList<string>> GetStoredBuildIds(string? artifactName = null);
}
