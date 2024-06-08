namespace DecSm.Atom.AzureStorage;

public sealed class AzureBlobArtifactProvider(IParamService paramService, IFileSystem fileSystem) : IArtifactProvider
{
    public IReadOnlyList<string> RequiredParams =>
    [
        nameof(IAzureArtifactStorage.AzureArtifactStorageContainer), nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString),
    ];
    
    public async Task UploadArtifact(string artifactName)
    {
        var artifactDir = fileSystem.ArtifactDirectory() / artifactName;
        
        var buildId = paramService.GetParam(nameof(ISetup.AtomBuildId));
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        
        var repoName = fileSystem.SolutionRoot()
            .DirectoryName;
        
        var artifactBlobDir = $"{repoName}/{buildId}/{artifactName}";
        var containerClient = new BlobContainerClient(connectionString, container);
        
        foreach (var file in fileSystem.Directory.EnumerateFiles(artifactDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = fileSystem.Path.GetRelativePath(artifactDir, file);
            var blobPath = $"{artifactBlobDir}/{relativePath}";
            
            var blobClient = containerClient.GetBlobClient(blobPath);
            
            await blobClient.UploadAsync(file, true);
        }
    }
    
    public async Task DownloadArtifact(string artifactName)
    {
        var artifactDir = fileSystem.ArtifactDirectory() / artifactName;
        
        var buildId = paramService.GetParam(nameof(ISetup.AtomBuildId));
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        
        var repoName = fileSystem.SolutionRoot()
            .DirectoryName;
        
        var containerClient = new BlobContainerClient(connectionString, container);
        
        if (artifactDir.DirectoryExists)
            fileSystem.Directory.Delete(artifactDir, true);
        
        fileSystem.Directory.CreateDirectory(artifactDir);
        
        foreach (var blobItem in containerClient.GetBlobsByHierarchy())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Blob.Name);
            
            var blobDownloadInfo = await blobClient.DownloadAsync();
            
            var blobName = blobItem.Blob.Name;
            
            if (blobName.StartsWith($"{repoName}/"))
                blobName = blobName.Substring($"{repoName}/".Length);
            
            if (blobName.StartsWith($"{buildId}/"))
                blobName = blobName.Substring($"{buildId}/".Length);
            
            if (blobName.StartsWith($"{artifactName}/"))
                blobName = blobName.Substring($"{artifactName}/".Length);
            
            var blobPath = fileSystem.Path.Combine(artifactDir, blobName);
            
            var blobPathParts = blobPath
                .Replace("/",
                    fileSystem
                        .Path
                        .DirectorySeparatorChar
                        .ToString()
                        .Replace("\\", ""))
                .Split(fileSystem.Path.DirectorySeparatorChar)
                .ToArray();
            
            if (!artifactDir.DirectoryExists)
                fileSystem.Directory.CreateDirectory(artifactDir);
            
            var blobPathParent = artifactDir.Path;
            
            for (var i = 0; i < blobPathParts.Length - 1; i++)
            {
                var blobPathPart = blobPathParts[i];
                blobPathParent = fileSystem.Path.Combine(blobPathParent, blobPathPart);
                
                if (artifactDir.Path.StartsWith(blobPathParent))
                    continue;
                
                if (!fileSystem.Directory.Exists(blobPathParent))
                    fileSystem.Directory.CreateDirectory(blobPathParent);
            }
            
            await using var fileStream = fileSystem.File.Create(blobPath);
            await blobDownloadInfo.Value.Content.CopyToAsync(fileStream);
        }
    }
}