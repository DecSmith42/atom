namespace DecSm.Atom.Module.DotnetHelpers;

public interface IDotnetPackHelper2 : IUseDotnetCli, IBuildInfo
{
    async Task DotnetPackAndStage(
        RootedPath projectPath,
        DotnetPackOptions2? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new();
        var configuration = options.PackOptions?.Configuration ?? "Release";

        var projectName = projectPath.FileNameWithoutExtension;

        var buildDirectory = options.PackOptions?.Output is { Length: > 0 }
            ? FileSystem.CreateRootedPath(options.PackOptions?.Output!)
            : projectPath.Parent! / "bin" / configuration;

        var publishDirectory = FileSystem.AtomPublishDirectory / projectName;

        Logger.LogInformation("Packing project {Project}", projectName);

        if (FileSystem.Directory.Exists(buildDirectory))
        {
            Logger.LogDebug("Deleting existing pack directory {PackDirectory}", buildDirectory);
            FileSystem.Directory.Delete(buildDirectory, true);
        }

        Logger.LogDebug(
            "Transforming project properties: SetVersionsFromProviders={SetVersionsFromProviders}, CustomPropertiesTransform={CustomPropertiesTransform}",
            options.SetVersionsFromProviders,
            options.CustomPropertiesTransform is not null
                ? "true"
                : "false");

        await using var transformFilesScope =
            (options.SetVersionsFromProviders, options.CustomPropertiesTransform) switch
            {
                (true, not null) => await TransformProjectVersionScope
                    .CreateAsync(DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                        BuildVersion,
                        cancellationToken)
                    .AddAsync(options.CustomPropertiesTransform),

                (true, null) => await TransformProjectVersionScope.CreateAsync(
                    DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    BuildVersion,
                    cancellationToken),

                (false, not null) => await TransformMultiFileScope.CreateAsync(
                    DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    options.CustomPropertiesTransform!,
                    cancellationToken),

                _ => null,
            };

        await DotnetCli.Pack(projectPath, options.PackOptions, cancellationToken);

        var packagedFile = FileSystem.CreateRootedPath(FileSystem
            .Directory
            .GetFiles(buildDirectory, $"{projectName}.*.nupkg")
            .OrderDescending()
            .First());

        if (!packagedFile.FileExists)
            throw new StepFailedException($"Package {packagedFile} does not exist.");

        Logger.LogDebug("Found packaged file {PackagedFile}", packagedFile);

        var packagedFileName = packagedFile.FileName!;
        var publishedFile = publishDirectory / packagedFileName;

        Logger.LogDebug("Moving package {PackagedFile} to {PublishedFile}", packagedFile, publishedFile);

        if (options.ClearPublishDirectory && FileSystem.Directory.Exists(publishDirectory))
        {
            Logger.LogDebug("Deleting existing publish directory {PublishDirectory}", publishDirectory);
            FileSystem.Directory.Delete(publishDirectory, true);
        }

        FileSystem.Directory.CreateDirectory(publishDirectory);

        if (FileSystem.File.Exists(publishedFile))
        {
            Logger.LogDebug("Deleting existing published file {PublishedFile}", publishedFile);
            FileSystem.File.Delete(publishedFile);
        }

        FileSystem.File.Move(packagedFile, publishedFile);

        Logger.LogInformation("Packed project {Project}", projectName);
    }
}

public sealed record DotnetPackOptions2
{
    public PackOptions? PackOptions { get; init; }

    public bool SetVersionsFromProviders { get; init; } = true;

    public Func<string, string>? CustomPropertiesTransform { get; init; }

    public bool ClearPublishDirectory { get; init; } = true;
}
