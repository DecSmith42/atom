namespace DecSm.Atom.Artifacts;

public interface IArtifactProvider
{
    IReadOnlyList<string> RequiredParams => [];

    Task UploadArtifacts(string[] artifactNames);

    Task DownloadArtifacts(string[] artifactNames);
}