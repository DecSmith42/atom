using DecSm.Atom.BuildInfo;

namespace DecSm.Atom.Module.AzureStorage;

[PublicAPI]
public sealed class AzureBlobArtifactProvider(
    IParamService paramService,
    ReportService reportService,
    IAtomFileSystem fileSystem,
    IBuildIdProvider buildIdProvider,
    ILogger<AzureBlobArtifactProvider> logger
) : IArtifactProvider
{
    public IReadOnlyList<string> RequiredParams =>
    [
        nameof(IAzureArtifactStorage.AzureArtifactStorageContainer), nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString),
    ];

    public async Task StoreArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null, string? slice = null)
    {
        buildId ??= buildIdProvider.BuildId;

        if (buildId is null)
            throw new InvalidOperationException("A run identifier is required to upload artifacts");

        var buildIdPathPrefix = buildIdProvider.GetBuildIdGroup(buildId);
        var buildIdPath = $"{buildIdPathPrefix}/{buildId}";

        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = paramService.GetParam(nameof(IBuildInfo.BuildName));

        var serviceClient = new BlobServiceClient(connectionString,
            new()
            {
                Retry =
                {
                    NetworkTimeout = TimeSpan.FromMinutes(3),
                },
            });

        var containerClient = serviceClient.GetBlobContainerClient(container);

        var invalidPathChars = fileSystem.Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        slice ??= pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildInfo.BuildSlice)) ?? string.Empty, "-");

        logger.LogInformation("Uploading artifacts '{Artifacts}' to container '{Container}' with path '{BlobPath}'",
            artifactNames,
            container.SanitizeForLogging(),
            slice is { Length: > 0 }
                ? $"{buildName}/{buildIdPath}/*/{slice}"
                : $"{buildName}/{buildIdPath}/*");

        foreach (var artifactName in artifactNames)
        {
            var publishDir = fileSystem.AtomPublishDirectory / artifactName;

            var artifactBlobDir = slice is { Length: > 0 }
                ? $"{buildName}/{buildIdPath}/{artifactName}/{slice}"
                : $"{buildName}/{buildIdPath}/{artifactName}";

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

    public async Task RetrieveArtifacts(IReadOnlyList<string> artifactNames, string? buildId = null, string? buildSlice = null)
    {
        buildId ??= buildIdProvider.BuildId;

        if (buildId is null)
            throw new InvalidOperationException("Build ID is required to upload artifacts");

        var buildIdPathPrefix = buildIdProvider.GetBuildIdGroup(buildId);
        var buildIdPath = $"{buildIdPathPrefix}/{buildId}";

        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = paramService.GetParam(nameof(IBuildInfo.BuildName));

        var serviceClient = new BlobServiceClient(connectionString,
            new()
            {
                Retry =
                {
                    NetworkTimeout = TimeSpan.FromMinutes(3),
                },
            });

        var containerClient = serviceClient.GetBlobContainerClient(container);

        var invalidPathChars = fileSystem.Path.GetInvalidPathChars();
        var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
        buildSlice ??= pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildInfo.BuildSlice)) ?? string.Empty, "-");

        logger.LogInformation("Downloading artifacts '{Artifacts}' from container '{Container}' with path '{BlobPath}'",
            artifactNames,
            container.SanitizeForLogging(),
            buildSlice is { Length: > 0 }
                ? $"{buildName}/{buildIdPath}/*/{buildSlice}"
                : $"{buildName}/{buildIdPath}/*");

        foreach (var artifactName in artifactNames)
        {
            var artifactDir = fileSystem.AtomArtifactsDirectory / artifactName;

            if (artifactDir.DirectoryExists)
                fileSystem.Directory.Delete(artifactDir, true);

            fileSystem.Directory.CreateDirectory(artifactDir);

            // Includes path separator at the end to prevent matching other directories with the same start of the name
            var artifactBlobDir = buildSlice is { Length: > 0 }
                ? $"{buildName}/{buildIdPath}/{artifactName}/{buildSlice}/"
                : $"{buildName}/{buildIdPath}/{artifactName}/";

            var hasAtLeastOneBlob = false;

            var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: artifactBlobDir);

            await foreach (var blobItem in blobs)
            {
                hasAtLeastOneBlob = true;

                logger.LogDebug("Downloading blob {Blob}", blobItem.Blob.Name);

                var blobClient = containerClient.GetBlobClient(blobItem.Blob.Name);

                var blobDownloadInfo = await blobClient.DownloadAsync();
                var blobName = blobItem.Blob.Name;

                if (blobName.StartsWith(artifactBlobDir))
                    blobName = blobName[artifactBlobDir.Length..];
                else
                    throw new InvalidOperationException($"Blob name {blobName} does not start with {buildName}/{buildIdPath}");

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
                    $"Could not find any blobs in the container {container} with the prefix {buildName}/{buildIdPath}/{artifactName}");
        }
    }

    public async Task Cleanup(IReadOnlyList<string> runIdentifiers)
    {
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = paramService.GetParam(nameof(ISetupBuildInfo.BuildName));
        var containerClient = new BlobContainerClient(connectionString, container);

        foreach (var buildId in runIdentifiers)
        {
            var buildIdPathPrefix = buildIdProvider.GetBuildIdGroup(buildId);
            var buildIdPath = $"{buildIdPathPrefix}/{buildId}";

            var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: $"{buildName}/{buildIdPath}/");

            await foreach (var blob in blobs)
            {
                logger.LogInformation("Deleting blob {Blob}", blob.Blob.Name);

                var blobClient = containerClient.GetBlobClient(blob.Blob.Name);

                await blobClient.DeleteIfExistsAsync();
            }
        }
    }

    public async Task<IReadOnlyList<string>> GetStoredRunIdentifiers(string? artifactName = null, string? buildSlice = null)
    {
        var connectionString = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageConnectionString));
        var container = paramService.GetParam(nameof(IAzureArtifactStorage.AzureArtifactStorageContainer));
        var buildName = paramService.GetParam(nameof(ISetupBuildInfo.BuildName));
        var containerClient = new BlobContainerClient(connectionString, container);

        if (artifactName is { Length: > 0 })
        {
            var invalidPathChars = fileSystem.Path.GetInvalidPathChars();
            var pathSafeRegex = new Regex($"[{Regex.Escape(new(invalidPathChars))}]");
            buildSlice ??= pathSafeRegex.Replace(paramService.GetParam(nameof(IBuildInfo.BuildSlice)) ?? string.Empty, "-");

            artifactName = buildSlice is { Length: > 0 }
                ? $"{artifactName}/{buildSlice}/"
                : $"{artifactName}/";
        }

        var blobs = containerClient.GetBlobsByHierarchyAsync(prefix: $"{buildName}/");

        var buildIds = new List<string>();

        var blobNameRegex = artifactName is { Length: > 0 }
            ? new Regex($"^{Regex.Escape(buildName ?? string.Empty)}/([^/]+/)+{Regex.Escape(artifactName)}")
            : new($"^{Regex.Escape(buildName ?? string.Empty)}/([^/]+/)+");

        await foreach (var blob in blobs)
        {
            var blobName = blob.Blob.Name;
            var match = blobNameRegex.Match(blobName);

            if (!match.Success)
                continue;

            var buildId = match
                .Groups[1]
                .Value
                .Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Last();

            if (!buildIds.Contains(buildId))
                buildIds.Add(buildId);
        }

        return buildIds;
    }
}
