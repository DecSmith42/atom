namespace DecSm.Atom.Module.Dotnet.Helpers;

[PublicAPI]
public interface IDotnetPublishHelper : IDotnetCliHelper, IBuildInfo
{
    Task DotnetPublishAndStage(
        string projectName,
        DotnetPublishAndStageOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var projectPath = DotnetFileUtil.GetProjectFilePathByName(FileSystem, projectName) ??
                          throw new StepFailedException($"Could not locate project file for project {projectName}.");

        Logger.LogDebug("Located project file for project {ProjectName} at {ProjectPath}", projectName, projectPath);

        return DotnetPublishAndStage(projectPath, options, cancellationToken);
    }

    async Task DotnetPublishAndStage(
        RootedPath projectPath,
        DotnetPublishAndStageOptions? options = null,
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
                    .CreateAsync(DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                        BuildVersion,
                        cancellationToken)
                    .AddAsync(options.CustomPropertiesTransform),

                (true, null) => await TransformProjectVersionScope.CreateAsync(
                    DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    BuildVersion,
                    cancellationToken),

                (false, not null) => await TransformMultiFileScope.CreateAsync(
                    DotnetFileUtil.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
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
            cancellationToken: cancellationToken);

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

[PublicAPI]
public sealed record DotnetPublishAndStageOptions
{
    public PublishOptions? PublishOptions { get; init; }

    public bool SetVersionsFromProviders { get; init; } = true;

    public Func<string, string>? CustomPropertiesTransform { get; init; }
}
