namespace DecSm.Atom.Artifacts;

public interface IArtifactProvider
{
    IReadOnlyList<string> RequiredParams => [];

    Task UploadArtifacts(string[] artifactNames, string? buildId = null);

    Task DownloadArtifacts(string[] artifactNames, string? buildId = null);

    Task DownloadArtifact(string artifactName, string[] buildIds);

    Task Cleanup(string[] buildIds);

    Task<IReadOnlyList<string>> GetStoredBuildIds(string? artifactName = null);
}