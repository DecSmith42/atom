namespace DecSm.Atom.Module.Dotnet.Helpers;

public interface IDotnetPackHelper : IDotnetCliHelper, IBuildInfo
{
    Task DotnetPackAndStage(
        string projectName,
        DotnetPackAndStageOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var projectPath = DotnetFileUtil.GetProjectFilePathByName(FileSystem, projectName) ??
                          throw new StepFailedException($"Could not locate project file for project {projectName}.");

        Logger.LogDebug("Located project file for project {ProjectName} at {ProjectPath}", projectName, projectPath);

        return DotnetPackAndStage(projectPath, options, cancellationToken);
    }

    async Task DotnetPackAndStage(
        RootedPath projectPath,
        DotnetPackAndStageOptions? options = null,
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

        await DotnetCli.Pack(projectPath, options.PackOptions, cancellationToken: cancellationToken);

        var packagedFile = FileSystem.CreateRootedPath(FileSystem
            .Directory
            .GetFiles(buildDirectory, $"{projectName}.*.nupkg")
            .OrderDescending()
            .First());

        if (!packagedFile.FileExists)
            throw new StepFailedException($"Package {packagedFile} does not exist.");

        Logger.LogDebug("Found packaged file {PackagedFile}", packagedFile);

        var publishedFile = publishDirectory / packagedFile.FileName!;

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

[PublicAPI]
public sealed record DotnetPackAndStageOptions
{
    public PackOptions? PackOptions { get; init; }

    public bool SetVersionsFromProviders { get; init; } = true;

    public Func<string, string>? CustomPropertiesTransform { get; init; }

    public bool ClearPublishDirectory { get; init; } = true;
}
