namespace DecSm.Atom.AzureStorage;

public sealed class AzureBlobArtifactProvider(IParamService paramService, IFileSystem fileSystem, ILogger<AzureBlobArtifactProvider> logger)
    : IArtifactProvider
{
    public IReadOnlyList<string> RequiredParams =>
    [
        nameof(IAzureArtifactStorage.AzureArtifactStorageContainer), nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString),
    ];
    
    public async Task UploadArtifact(string artifactName)
    {
        var publishDir = fileSystem.PublishDirectory() / artifactName;
        
        var buildId = paramService.GetParam(nameof(ISetup.AtomBuildId));
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        
        var solutionName = fileSystem.SolutionName();
        
        var artifactBlobDir = $"{solutionName}/{buildId}/{artifactName}";
        var containerClient = new BlobContainerClient(connectionString, container);
        
        var files = fileSystem.Directory.GetFiles(publishDir, "*", SearchOption.AllDirectories);
        
        foreach (var file in files)
            logger.LogInformation("Uploading file {File}", file);
        
        if (files.Length == 0)
            throw new InvalidOperationException($"Could not find any files in the directory {publishDir}");
        
        foreach (var file in files)
        {
            var relativePath = fileSystem.Path.GetRelativePath(publishDir, file);
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
        
        var solutionName = fileSystem.SolutionName();
        
        var containerClient = new BlobContainerClient(connectionString, container);
        
        if (artifactDir.DirectoryExists)
            fileSystem.Directory.Delete(artifactDir, true);
        
        fileSystem.Directory.CreateDirectory(artifactDir);
        
        var blobs = containerClient
            .GetBlobsByHierarchy(prefix: $"{solutionName}/{buildId}/{artifactName}")
            .ToArray();
        
        foreach (var blob in blobs)
            logger.LogInformation("Downloading blob {Blob}", blob.Blob.Name);
        
        if (blobs.Length == 0)
            throw new InvalidOperationException(
                $"Could not find any blobs in the container {container} with the prefix {solutionName}/{buildId}/{artifactName}");
        
        foreach (var blobItem in blobs)
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Blob.Name);
            
            var blobDownloadInfo = await blobClient.DownloadAsync();
            
            var blobName = blobItem.Blob.Name;
            
            if (blobName.StartsWith($"{solutionName}/"))
                blobName = blobName[$"{solutionName}/".Length..];
            
            if (blobName.StartsWith($"{buildId}/"))
                blobName = blobName[$"{buildId}/".Length..];
            
            if (blobName.StartsWith($"{artifactName}/"))
                blobName = blobName[$"{artifactName}/".Length..];
            
            var blobPath = fileSystem.Path.Combine(artifactDir, blobName);
            
            var blobPathParts = blobPath
                .Replace("/",
                    fileSystem
                        .Path
                        .DirectorySeparatorChar
                        .ToString()
                        .Replace("\\", fileSystem.Path.DirectorySeparatorChar.ToString()))
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