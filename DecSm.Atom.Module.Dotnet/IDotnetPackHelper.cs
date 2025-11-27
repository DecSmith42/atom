namespace DecSm.Atom.Module.Dotnet;

public interface IDotnetPackHelper : IBuildAccessor
{
    async Task DotnetPackProject(DotnetPackOptions options, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Packing Atom project {AtomProjectName}", options.ProjectName);

        var projectPath = DotnetFileUtils.GetProjectFilePathByName(FileSystem, options.ProjectName);

        await using var transformFilesScope = (options.AutoSetVersion, options.CustomPropertiesTransform) switch
        {
            (true, not null) => await TransformProjectVersionScope
                .CreateAsync(DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    GetService<IBuildVersionProvider>()
                        .Version,
                    cancellationToken)
                .AddAsync(options.CustomPropertiesTransform),

            (true, null) => await TransformProjectVersionScope.CreateAsync(
                DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                GetService<IBuildVersionProvider>()
                    .Version,
                cancellationToken),

            (false, not null) => await TransformMultiFileScope.CreateAsync(
                DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                options.CustomPropertiesTransform!,
                cancellationToken),

            _ => null,
        };

        var packDirectory = FileSystem.AtomRootDirectory / options.ProjectName / "bin" / options.Configuration;

        if (FileSystem.Directory.Exists(packDirectory))
            FileSystem.Directory.Delete(packDirectory, true);

        var command = options switch
        {
            { RuntimeIdentifier: not null } =>
                $"pack {projectPath.Path} -c {options.Configuration} -r {options.RuntimeIdentifier} /p:PublishAot={options.NativeAot.ToString().ToLower()}",
            _ => $"pack {projectPath.Path} -c {options.Configuration}",
        };

        await ProcessRunner.RunAsync(new("dotnet", command), cancellationToken);

        var packageFilePattern = options.CustomPackageId is { Length: > 0 }
            ? $"{options.CustomPackageId}.*.nupkg"
            : $"{options.ProjectName}.*.nupkg";

        var packageFileName = FileSystem
            .Directory
            .GetFiles(packDirectory, packageFilePattern)
            .OrderByDescending(x => x)
            .First();

        // Move package to publish directory
        var packagePath = packDirectory / $"{packageFileName}";

        if (!packagePath.FileExists)
            throw new InvalidOperationException($"Package {packagePath} does not exist.");

        var outputArtifactName = options.OutputArtifactName ?? options.ProjectName;
        var publishDir = FileSystem.AtomPublishDirectory / outputArtifactName;

        Logger.LogInformation("Moving package {PackagePath} to {PublishDir}",
            packagePath,
            publishDir / packagePath.FileName!);

        if (!options.SuppressClearingPublishDirectory && FileSystem.Directory.Exists(publishDir))
            FileSystem.Directory.Delete(publishDir, true);

        FileSystem.Directory.CreateDirectory(publishDir);

        if (FileSystem.File.Exists(publishDir / packagePath.FileName!))
        {
            Logger.LogDebug("Deleting existing package {PackagePath}", publishDir / packagePath.FileName!);
            FileSystem.File.Delete(publishDir / packagePath.FileName!);
        }

        FileSystem.File.Move(packagePath, publishDir / packagePath.FileName!);

        Logger.LogInformation("Packed Atom project {AtomProjectName}", options.ProjectName);
    }
}
