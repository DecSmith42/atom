namespace DecSm.Atom.Module.DotnetHelpers;

public interface IDotnetPublishHelper2 : IUseDotnetCli, IBuildInfo
{
    async Task DotnetPublishAndStage(
        RootedPath projectPath,
        DotnetPublishOptions2? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new();
        var configuration = options.PublishOptions?.Configuration ?? "Release";

        var projectName = projectPath.FileNameWithoutExtension;

        var buildDirectory = options.PublishOptions?.Output is { Length: > 0 }
            ? FileSystem.CreateRootedPath(options.PublishOptions?.Output!)
            : FileSystem.AtomRootDirectory / projectName / configuration / "atom-publish";

        var publishDirectory = FileSystem.AtomPublishDirectory / projectName;

        Logger.LogInformation("Publishing project {Project}", projectName);

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

        await DotnetCli.Publish(projectPath,
            options.PublishOptions is null
                ? new()
                {
                    Output = buildDirectory,
                }
                : options.PublishOptions with
                {
                    Output = buildDirectory,
                },
            cancellationToken);

        Logger.LogDebug("Moving files in {BuiltDirectory} to {PublishedDirectory}", buildDirectory, publishDirectory);

        if (FileSystem.Directory.Exists(publishDirectory))
        {
            Logger.LogDebug("Deleting existing publish directory {PublishDirectory}", publishDirectory);
            FileSystem.Directory.Delete(publishDirectory, true);
        }

        if (!FileSystem.Directory.Exists(publishDirectory.Parent!))
            FileSystem.Directory.CreateDirectory(publishDirectory.Parent!);

        FileSystem.Directory.Move(buildDirectory, publishDirectory);

        Logger.LogInformation("Published project {Project}", projectName);
    }
}

public sealed record DotnetPublishOptions2
{
    public PublishOptions? PublishOptions { get; init; }

    public bool SetVersionsFromProviders { get; init; } = true;

    public Func<string, string>? CustomPropertiesTransform { get; init; }
}
