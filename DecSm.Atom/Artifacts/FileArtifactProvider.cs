namespace DecSm.Atom.Artifacts;

public class FileArtifactProvider(IFileSystem fileSystem) : IArtifactProvider
{
    private readonly AbsolutePath _artifactStoreDirectory =
        new(fileSystem, $@"%TEMP%\DecSm\Artifacts\{fileSystem.RepoRoot().DirectoryName}");
    
    public Task UploadArtifact(string artifactName)
    {
        var artifact = fileSystem.ArtifactDirectory() / artifactName;
        
        if (artifact.FileExists)
        {
            fileSystem.File.Copy(artifact, _artifactStoreDirectory / artifactName);
            
            return Task.CompletedTask;
        }
        
        if (artifact.DirectoryExists)
        {
            fileSystem.Directory.Copy(artifact, _artifactStoreDirectory / artifactName);
            
            return Task.CompletedTask;
        }
        
        throw new InvalidOperationException($"Artifact '{artifactName}' does not exist.");
    }
    
    public Task DownloadArtifact(string artifactName)
    {
        var artifact = _artifactStoreDirectory / artifactName;
        
        if (artifact.FileExists)
        {
            fileSystem.File.Copy(artifact, fileSystem.ArtifactDirectory() / artifactName);
            
            return Task.CompletedTask;
        }
        
        if (artifact.DirectoryExists)
        {
            fileSystem.Directory.Copy(artifact, fileSystem.ArtifactDirectory() / artifactName);
            
            return Task.CompletedTask;
        }
        
        throw new InvalidOperationException($"Artifact '{artifactName}' does not exist.");
    }
}