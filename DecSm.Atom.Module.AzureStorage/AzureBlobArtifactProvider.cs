namespace DecSm.Atom.Module.AzureStorage;

public sealed class AzureBlobArtifactProvider(
    IParamService paramService,
    IReportService reportService,
    IAtomFileSystem fileSystem,
    ILogger<AzureBlobArtifactProvider> logger
) : IArtifactProvider
{
    public IReadOnlyList<string> RequiredParams =>
    [
        nameof(IAzureArtifactStorage.AzureArtifactStorageContainer), nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString),
    ];

    public async Task UploadArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null)
    {
        buildId ??= paramService.GetParam(nameof(ISetup.AtomBuildId));

        if (buildId is null)
            throw new InvalidOperationException("Build ID is required to upload artifacts");

        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = paramService.GetParam(nameof(ISetup.AtomBuildName));
        var containerClient = new BlobContainerClient(connectionString, container);

        var invalidPathChars = fileSystem.Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        var matrixSlice = pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildDefinition.MatrixSlice)) ?? string.Empty, "-");

        logger.LogInformation("Uploading artifacts '{Artifacts}' to container '{Container}'",
            artifactNames,
            container.SanitizeForLogging());

        foreach (var artifactName in artifactNames)
        {
            var publishDir = fileSystem.AtomPublishDirectory / artifactName;

            var artifactBlobDir = matrixSlice is { Length: > 0 }
                ? $"{buildName}/{buildId}/{artifactName}/{matrixSlice}"
                : $"{buildName}/{buildId}/{artifactName}";

            var files = fileSystem.Directory.GetFiles(publishDir, "*", SearchOption.AllDirectories);

            foreach (var file in files)
                logger.LogDebug("Uploading file {File}", file);

            if (files.Length == 0)
                throw new InvalidOperationException($"Could not find any files in the directory {publishDir}");

            logger.LogInformation("Uploading {FileCount} files to {BlobDir}", files.Length, artifactBlobDir.SanitizeForLogging());

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

    public async Task DownloadArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null)
    {
        buildId ??= paramService.GetParam(nameof(ISetup.AtomBuildId));
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = paramService.GetParam(nameof(ISetup.AtomBuildName));
        var containerClient = new BlobContainerClient(connectionString, container);

        var invalidPathChars = fileSystem.Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        var matrixSlice = pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildDefinition.MatrixSlice)) ?? string.Empty, "-");

        logger.LogInformation("Downloading artifacts '{Artifacts}' from container '{Container}'",
            artifactNames,
            container.SanitizeForLogging());

        foreach (var artifactName in artifactNames)
        {
            var artifactDir = fileSystem.AtomArtifactsDirectory / artifactName;

            if (artifactDir.DirectoryExists)
                fileSystem.Directory.Delete(artifactDir, true);

            fileSystem.Directory.CreateDirectory(artifactDir);

            // Includes path separator at the end to prevent matching other directories with the same start of the name
            var artifactBlobDir = matrixSlice is { Length: > 0 }
                ? $"{buildName}/{buildId}/{artifactName}/{matrixSlice}/"
                : $"{buildName}/{buildId}/{artifactName}/";

            var hasAtLeastOneBlob = false;

            var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: artifactBlobDir);

            await foreach (var blobItem in blobs)
            {
                hasAtLeastOneBlob = true;

                logger.LogDebug("Downloading blob {Blob}", blobItem.Blob.Name);

                var blobClient = containerClient.GetBlobClient(blobItem.Blob.Name);

                var blobDownloadInfo = await blobClient.DownloadAsync();
                var blobName = blobItem.Blob.Name;

                if (blobName.StartsWith($"{buildName}/{buildId}/{artifactName}/"))
                    blobName = blobName[$"{buildName}/{buildId}/{artifactName}/".Length..];
                else
                    throw new InvalidOperationException($"Blob name {blobName} does not start with {buildName}/{buildId}");

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
                    $"Could not find any blobs in the container {container} with the prefix {buildName}/{buildId}/{artifactName}");
        }
    }

    public async Task DownloadArtifact(string artifactName, IReadOnlyList<string> buildIds)
    {
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = paramService.GetParam(nameof(ISetup.AtomBuildName));
        var containerClient = new BlobContainerClient(connectionString, container);

        var invalidPathChars = fileSystem.Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        var matrixSlice = pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildDefinition.MatrixSlice)) ?? string.Empty, "-");

        logger.LogInformation("Downloading artifact '{Artifacts}' from container '{Container}'", artifactName, container);

        foreach (var buildId in buildIds)
        {
            var artifactDir = fileSystem.AtomArtifactsDirectory / buildId / artifactName;

            if (artifactDir.DirectoryExists)
                fileSystem.Directory.Delete(artifactDir, true);

            fileSystem.Directory.CreateDirectory(artifactDir);

            var artifactBlobDir = matrixSlice is { Length: > 0 }
                ? $"{buildName}/{buildId}/{artifactName}/{matrixSlice}/"
                : $"{buildName}/{buildId}/{artifactName}/";

            var hasAtLeastOneBlob = false;

            var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: artifactBlobDir);

            await foreach (var blobItem in blobs)
            {
                hasAtLeastOneBlob = true;

                logger.LogDebug("Downloading blob {Blob}", blobItem.Blob.Name);

                var blobClient = containerClient.GetBlobClient(blobItem.Blob.Name);

                var blobDownloadInfo = await blobClient.DownloadAsync();
                var blobName = blobItem.Blob.Name;

                if (blobName.StartsWith($"{buildName}/{buildId}/{artifactName}/"))
                    blobName = blobName[$"{buildName}/{buildId}/{artifactName}/".Length..];
                else
                    throw new InvalidOperationException($"Blob name {blobName} does not start with {buildName}/{buildId}");

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
                    $"Could not find any blobs in the container {container} with the prefix {buildName}/{buildId}/{artifactName}");
        }
    }

    public async Task Cleanup(IReadOnlyList<string> buildIds)
    {
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = paramService.GetParam(nameof(ISetup.AtomBuildName));
        var containerClient = new BlobContainerClient(connectionString, container);

        foreach (var buildId in buildIds)
        {
            var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: $"{buildName}/{buildId}/");

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
        var buildName = paramService.GetParam(nameof(ISetup.AtomBuildName));
        var containerClient = new BlobContainerClient(connectionString, container);

        if (artifactName is { Length: > 0 })
        {
            var invalidPathChars = fileSystem.Path.GetInvalidPathChars();
            var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
            var matrixSlice = pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildDefinition.MatrixSlice)) ?? string.Empty, "-");

            artifactName = matrixSlice is { Length: > 0 }
                ? $"{artifactName}/{matrixSlice}/"
                : $"{artifactName}/";
        }

        var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: $"{buildName}/");

        var buildIds = new List<string>();

        var blobNameRegex = artifactName is { Length: > 0 }
            ? new Regex($"^{buildName}/[^/]+/{artifactName}")
            : new($"^{buildName}/[^/]+/");

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
