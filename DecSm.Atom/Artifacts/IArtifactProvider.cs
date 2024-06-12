namespace DecSm.Atom.Artifacts;

public interface IArtifactProvider
{
    Task UploadArtifacts(string[] artifactNames);
    
    Task DownloadArtifacts(string[] artifactNames);
    
    IReadOnlyList<string> RequiredParams => [];
}