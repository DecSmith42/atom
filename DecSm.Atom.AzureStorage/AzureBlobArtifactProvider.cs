namespace DecSm.Atom.AzureStorage;

public sealed class AzureBlobArtifactProvider(IParamService paramService, IFileSystem fileSystem) : IArtifactProvider
{
    public IReadOnlyList<string> RequiredParams =>
    [
        nameof(IAzureArtifactStorage.AzureArtifactStorageContainer), nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString),
    ];
    
    public async Task UploadArtifact(string artifactName)
    {
        var artifact = fileSystem.ArtifactDirectory() / artifactName;
        var cleanZip = false;
        
        if (artifact.DirectoryExists)
        {
            artifact = fileSystem.TempDirectory() / "artifact.zip";
            
            if (artifact.FileExists)
                fileSystem.File.Delete(artifact);
            
            ZipFile.CreateFromDirectory(artifact, artifact);
            cleanZip = true;
        }
        
        if (!artifact.FileExists)
            throw new FileNotFoundException("Artifact file does not exist", artifact);
        
        var buildId = paramService.GetParam(nameof(ISetup.AtomBuildId));
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        
        var containerClient = new BlobContainerClient(connectionString, container);
        
        var repoName = fileSystem.RepoRoot()
            .DirectoryName;
        
        await using var fileStream = fileSystem.File.OpenRead(artifact);
        await containerClient.UploadBlobAsync($"{repoName}/{buildId}/{artifactName}", fileStream);
        
        if (cleanZip)
            fileSystem.File.Delete(artifact);
    }
    
    public async Task DownloadArtifact(string artifactName)
    {
        var buildId = paramService.GetParam(nameof(ISetup.AtomBuildId));
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        
        var containerClient = new BlobContainerClient(connectionString, container);
        
        var repoName = fileSystem.RepoRoot()
            .DirectoryName;
        
        var archive = await containerClient
            .GetBlobClient($"{repoName}/{buildId}/{artifactName}")
            .DownloadAsync();
        
        await using var archiveStream = archive.Value.Content;
        
        var artifact = fileSystem.ArtifactDirectory() / artifactName;
        
        await using var fileStream = fileSystem.File.OpenWrite(artifact);
        await archiveStream.CopyToAsync(fileStream);
    }
}