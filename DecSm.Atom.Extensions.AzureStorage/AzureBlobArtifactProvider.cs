using Azure.Storage.Blobs.Models;

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

    public async Task UploadArtifacts(string[] artifactNames, string? buildId = null)
    {
        buildId ??= paramService.GetParam(nameof(ISetup.AtomBuildId));
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var solutionName = fileSystem.SolutionName();
        var containerClient = new BlobContainerClient(connectionString, container);

        var invalidPathChars = Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        var matrixSlice = pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildDefinition.MatrixSlice)) ?? string.Empty, "-");

        foreach (var artifactName in artifactNames)
        {
            var publishDir = fileSystem.PublishDirectory() / artifactName;

            var artifactBlobDir = matrixSlice is { Length: > 0 }
                ? $"{solutionName}/{buildId}/{artifactName}/{matrixSlice}"
                : $"{solutionName}/{buildId}/{artifactName}";

            var files = fileSystem.Directory.GetFiles(publishDir, "*", SearchOption.AllDirectories);

            foreach (var file in files)
                logger.LogDebug("Uploading file {File}", file);

            if (files.Length == 0)
                throw new InvalidOperationException($"Could not find any files in the directory {publishDir}");

            foreach (var file in files)
            {
                var relativePath = fileSystem.Path.GetRelativePath(publishDir, file);
                var blobPath = $"{artifactBlobDir}/{relativePath}";

                var blobClient = containerClient.GetBlobClient(blobPath);

                await blobClient.UploadAsync(file,
                    new BlobHttpHeaders
                    {
                        ContentType = MimeTypes.GetMimeType(file),
                    });
            }

            // Add report data for the artifact - name and url
            reportService.AddReportData(new ArtifactReportData($"{artifactName} - {buildId}", $"{containerClient.Uri}/{artifactBlobDir}"));
        }
    }

    public async Task DownloadArtifacts(string[] artifactNames, string? buildId = null)
    {
        buildId ??= paramService.GetParam(nameof(ISetup.AtomBuildId));
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var solutionName = fileSystem.SolutionName();
        var containerClient = new BlobContainerClient(connectionString, container);

        var invalidPathChars = Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        var matrixSlice = pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildDefinition.MatrixSlice)) ?? string.Empty, "-");

        foreach (var artifactName in artifactNames)
        {
            var artifactDir = fileSystem.ArtifactDirectory() / artifactName;

            if (artifactDir.DirectoryExists)
                fileSystem.Directory.Delete(artifactDir, true);

            fileSystem.Directory.CreateDirectory(artifactDir);

            // Includes path separator at the end to prevent matching other directories with the same start of the name
            var artifactBlobDir = matrixSlice is { Length: > 0 }
                ? $"{solutionName}/{buildId}/{artifactName}/{matrixSlice}/"
                : $"{solutionName}/{buildId}/{artifactName}/";

            var hasAtLeastOneBlob = false;

            var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: artifactBlobDir);

            await foreach (var blobItem in blobs)
            {
                hasAtLeastOneBlob = true;

                logger.LogDebug("Downloading blob {Blob}", blobItem.Blob.Name);

                var blobClient = containerClient.GetBlobClient(blobItem.Blob.Name);

                var blobDownloadInfo = await blobClient.DownloadAsync();
                var blobName = blobItem.Blob.Name;

                if (blobName.StartsWith($"{solutionName}/{buildId}/{artifactName}/"))
                    blobName = blobName[$"{solutionName}/{buildId}/{artifactName}/".Length..];
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

            if (!hasAtLeastOneBlob)
                throw new InvalidOperationException(
                    $"Could not find any blobs in the container {container} with the prefix {solutionName}/{buildId}/{artifactName}");
        }
    }

    public async Task DownloadArtifact(string artifactName, string[] buildIds)
    {
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var solutionName = fileSystem.SolutionName();
        var containerClient = new BlobContainerClient(connectionString, container);

        var invalidPathChars = Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        var matrixSlice = pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildDefinition.MatrixSlice)) ?? string.Empty, "-");

        foreach (var buildId in buildIds)
        {
            var artifactDir = fileSystem.ArtifactDirectory() / buildId / artifactName;

            if (artifactDir.DirectoryExists)
                fileSystem.Directory.Delete(artifactDir, true);

            fileSystem.Directory.CreateDirectory(artifactDir);

            var artifactBlobDir = matrixSlice is { Length: > 0 }
                ? $"{solutionName}/{buildId}/{artifactName}/{matrixSlice}/"
                : $"{solutionName}/{buildId}/{artifactName}/";

            var hasAtLeastOneBlob = false;

            var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: artifactBlobDir);

            await foreach (var blobItem in blobs)
            {
                hasAtLeastOneBlob = true;

                logger.LogDebug("Downloading blob {Blob}", blobItem.Blob.Name);

                var blobClient = containerClient.GetBlobClient(blobItem.Blob.Name);

                var blobDownloadInfo = await blobClient.DownloadAsync();
                var blobName = blobItem.Blob.Name;

                if (blobName.StartsWith($"{solutionName}/{buildId}/{artifactName}/"))
                    blobName = blobName[$"{solutionName}/{buildId}/{artifactName}/".Length..];
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

            if (!hasAtLeastOneBlob)
                throw new InvalidOperationException(
                    $"Could not find any blobs in the container {container} with the prefix {solutionName}/{buildId}/{artifactName}");
        }
    }

    public async Task Cleanup(string[] buildIds)
    {
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var solutionName = fileSystem.SolutionName();
        var containerClient = new BlobContainerClient(connectionString, container);

        foreach (var buildId in buildIds)
        {
            var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: $"{solutionName}/{buildId}/");

            await foreach (var blob in blobs)
            {
                logger.LogInformation("Deleting blob {Blob}", blob.Blob.Name);

                var blobClient = containerClient.GetBlobClient(blob.Blob.Name);

                await blobClient.DeleteIfExistsAsync();
            }
        }
    }

    public async Task<IReadOnlyList<string>> GetStoredBuildIds(string? artifactName = null)
    {
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var solutionName = fileSystem.SolutionName();
        var containerClient = new BlobContainerClient(connectionString, container);

        if (artifactName is { Length: > 0 })
        {
            var invalidPathChars = Path.GetInvalidPathChars();
            var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
            var matrixSlice = pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildDefinition.MatrixSlice)) ?? string.Empty, "-");

            artifactName = matrixSlice is { Length: > 0 }
                ? $"{artifactName}/{matrixSlice}/"
                : $"{artifactName}/";
        }

        var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: $"{solutionName}/");

        var buildIds = new List<string>();

        var blobNameRegex = artifactName is { Length: > 0 }
            ? new Regex($"^{solutionName}/[^/]+/{artifactName}")
            : new($"^{solutionName}/[^/]+/");

        await foreach (var blob in blobs)
        {
            var blobName = blob.Blob.Name;

            if (!blobNameRegex.IsMatch(blobName))
                continue;

            var buildId = blobName
                .Split('/', StringSplitOptions.RemoveEmptyEntries)[1];

            if (!buildIds.Contains(buildId))
                buildIds.Add(buildId);
        }

        return buildIds;
    }
}