namespace DecSm.Atom.Module.GithubWorkflows;

[TargetDefinition]
public partial interface IGithubReleaseHelper : IGithubHelper
{
    async Task UploadArtifactToRelease(string artifactName, string releaseTag)
    {
        var artifactPath = FileSystem.AtomArtifactsDirectory / artifactName;

        await UploadAssetToRelease(releaseTag, artifactPath);
    }

    async Task UploadAssetToRelease(string releaseTag, RootedPath assetPath)
    {
        var client = new GitHubClient(new("DecSm.Atom"), new InMemoryCredentialStore(new(GithubToken)));

        var releases = await client.Repository.Release.GetAll(long.Parse(Github.Variables.RepositoryId));

        var release = releases.FirstOrDefault(x => x.TagName == releaseTag);

        if (assetPath.DirectoryExists)
        {
            var zipPath = FileSystem.CreateRootedPath($"{assetPath}.zip");
            ZipFile.CreateFromDirectory(assetPath, zipPath);
            assetPath = zipPath;
        }

        var assetFile = FileSystem.FileInfo.New(assetPath);

        if (!assetFile.FullName.EndsWith(".zip"))
        {
            // zip the file
            var zipPath = FileSystem.CreateRootedPath($"{assetPath}.zip");
            ZipFile.CreateFromDirectory(assetPath.Parent!, zipPath);
            assetPath = zipPath;
        }

        assetFile = FileSystem.FileInfo.New(assetPath);

        await using var stream = assetFile.OpenRead();

        var asset = new ReleaseAssetUpload(assetPath.FileName, "application/zip", stream, TimeSpan.FromMinutes(5));
        await client.Repository.Release.UploadAsset(release, asset);
    }
}
