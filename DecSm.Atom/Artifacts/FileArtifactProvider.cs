namespace DecSm.Atom.Artifacts;

public class FileArtifactProvider(IFileSystem fileSystem) : IArtifactProvider
{
    private readonly AbsolutePath _artifactStoreDirectory =
        new(fileSystem, $@"%TEMP%\DecSm\Artifacts\{fileSystem.RepoRoot().DirectoryName}");
    
    public Task UploadArtifacts(string[] artifactNames)
    {
        foreach (var artifactName in artifactNames)
        {
            var artifact = fileSystem.ArtifactDirectory() / artifactName;
            
            if (artifact.FileExists)
            {
                fileSystem.File.Copy(artifact, _artifactStoreDirectory / artifactName);
                
                continue;
            }
            
            if (artifact.DirectoryExists)
            {
                fileSystem.Directory.Copy(artifact, _artifactStoreDirectory / artifactName);
                
                continue;
            }
            
            throw new InvalidOperationException($"Artifact '{artifactName}' does not exist.");
        }
        
        return Task.CompletedTask;
    }
    
    public Task DownloadArtifacts(string[] artifactNames)
    {
        foreach (var artifactName in artifactNames)
        {
            var artifact = _artifactStoreDirectory / artifactName;
            
            if (artifact.FileExists)
            {
                fileSystem.File.Copy(artifact, fileSystem.ArtifactDirectory() / artifactName);
                
                continue;
            }
            
            if (artifact.DirectoryExists)
            {
                fileSystem.Directory.Copy(artifact, fileSystem.ArtifactDirectory() / artifactName);
                
                continue;
            }
            
            throw new InvalidOperationException($"Artifact '{artifactName}' does not exist.");
        }
        
        return Task.CompletedTask;
    }
}