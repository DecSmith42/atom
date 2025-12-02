namespace DecSm.Atom.Module.GithubWorkflows;

[TargetDefinition]
public partial interface IGithubReleaseHelper : IGithubHelper
{
    async Task UploadArtifactToRelease(
        string artifactName,
        string releaseTag,
        bool dryRunWhenNotRunningInGithubActions = true)
    {
        var artifactPath = FileSystem.AtomArtifactsDirectory / artifactName;

        await UploadAssetToRelease(releaseTag, artifactPath, dryRunWhenNotRunningInGithubActions);
    }

    async Task UploadAssetToRelease(
        string releaseTag,
        RootedPath assetPath,
        bool dryRunWhenNotRunningInGithubActions = true)
    {
        if (!Github.IsGithubActions && dryRunWhenNotRunningInGithubActions)
        {
            Logger.LogWarning(
                "Not running in GitHub Actions, simulating upload of artifact {AssetPath} to release {ReleaseTag}.",
                assetPath,
                releaseTag);

            if (assetPath.DirectoryExists)
                Logger.LogInformation("Artifact path {AssetPath} is a directory containing {FileCount} files.",
                    assetPath,
                    FileSystem.Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories)
                        .Length);
            else
                Logger.LogInformation("Artifact path {AssetPath} is a file.", assetPath);

            return;
        }

        var client = new GitHubClient(new("DecSm.Atom"), new InMemoryCredentialStore(new(GithubToken)));

        var releases = await client.Repository.Release.GetAll(long.Parse(Github.Variables.RepositoryId));

        var release = releases.FirstOrDefault(x => x.TagName == releaseTag);

        if (assetPath.DirectoryExists)
        {
            var zipPath = FileSystem.CreateRootedPath($"{assetPath}.zip");

            #if NET10_0_OR_GREATER
            await ZipFile.CreateFromDirectoryAsync(assetPath, zipPath);
            #else
            ZipFile.CreateFromDirectory(assetPath, zipPath);
            #endif

            assetPath = zipPath;
        }

        var assetFile = FileSystem.FileInfo.New(assetPath);

        if (!assetFile.FullName.EndsWith(".zip"))
        {
            // zip the file
            var zipPath = FileSystem.CreateRootedPath($"{assetPath}.zip");

            #if NET10_0_OR_GREATER
            await ZipFile.CreateFromDirectoryAsync(assetPath.Parent!, zipPath);
            #else
            ZipFile.CreateFromDirectory(assetPath.Parent!, zipPath);
            #endif

            assetPath = zipPath;
        }

        assetFile = FileSystem.FileInfo.New(assetPath);

        await using var stream = assetFile.OpenRead();

        var asset = new ReleaseAssetUpload(assetPath.FileName, "application/zip", stream, TimeSpan.FromMinutes(5));
        await client.Repository.Release.UploadAsset(release, asset);
    }
}
