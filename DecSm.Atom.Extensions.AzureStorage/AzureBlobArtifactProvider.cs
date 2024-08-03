namespace DecSm.Atom.Extensions.AzureStorage;

public sealed class AzureBlobArtifactProvider(
    IParamService paramService,
    IReportService reportService,
    IFileSystem fileSystem,
    ILogger<AzureBlobArtifactProvider> logger
) : IArtifactProvider
{
    public IReadOnlyList<string> RequiredParams =>
    [
        nameof(IAzureArtifactStorage.AzureArtifactStorageContainer), nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString),
    ];

    public async Task UploadArtifacts(string[] artifactNames)
    {
        var buildId = paramService.GetParam(nameof(ISetup.AtomBuildId));
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var solutionName = fileSystem.SolutionName();
        var containerClient = new BlobContainerClient(connectionString, container);

        var invalidPathChars = Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        var matrixSlice = pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildDefinition.MatrixSlice)) ?? string.Empty, "-");

        foreach (var artifactName in artifactNames)
        {
            var publishDir = matrixSlice is { Length: > 0 }
                ? fileSystem.PublishDirectory() / artifactName / matrixSlice
                : fileSystem.PublishDirectory() / artifactName;

            var artifactBlobDir = $"{solutionName}/{buildId}/{artifactName}";

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

            // Add report data for the artifact - name and url
            reportService.AddReportData(new ArtifactReportData($"{artifactName} - {buildId}", $"{containerClient.Uri}/{artifactBlobDir}"));
        }
    }

    public async Task DownloadArtifacts(string[] artifactNames)
    {
        var buildId = paramService.GetParam(nameof(ISetup.AtomBuildId));
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var solutionName = fileSystem.SolutionName();
        var containerClient = new BlobContainerClient(connectionString, container);

        var invalidPathChars = Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        var matrixSlice = pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildDefinition.MatrixSlice)) ?? string.Empty, "-");

        foreach (var artifactName in artifactNames)
        {
            var artifactDir = matrixSlice is { Length: > 0 }
                ? fileSystem.ArtifactDirectory() / artifactName / matrixSlice
                : fileSystem.ArtifactDirectory() / artifactName;

            if (artifactDir.DirectoryExists)
                fileSystem.Directory.Delete(artifactDir, true);

            fileSystem.Directory.CreateDirectory(artifactDir);

            var blobs = containerClient
                .GetBlobsByHierarchy(prefix: $"{solutionName}/{buildId}/{artifactName}/")
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

                if (blobName.StartsWith($"{solutionName}/{buildId}/{artifactName}/"))
                    blobName = blobName.Substring($"{solutionName}/{buildId}/{artifactName}/".Length);
                else
                    throw new InvalidOperationException($"Blob name {blobName} does not start with {solutionName}/{buildId}");

                var blobPath = artifactDir / blobName;
                var blobDir = fileSystem.Path.GetDirectoryName(blobPath);

                if (blobDir is null)
                    throw new InvalidOperationException($"Could not get directory name for blob path {blobPath}");

                if (!fileSystem.Directory.Exists(blobDir))
                    fileSystem.Directory.CreateDirectory(blobDir);

                logger.LogTrace("Writing file {BlobPath}", blobPath);
                await using var fileStream = fileSystem.File.Create(blobPath);
                await blobDownloadInfo.Value.Content.CopyToAsync(fileStream);
            }
        }
    }
}