namespace DecSm.Atom.Artifacts;

public interface IArtifactProvider
{
    Task UploadArtifact(string artifactName);
    
    Task DownloadArtifact(string artifactName);
    
    IReadOnlyList<string> RequiredParams => [];
}