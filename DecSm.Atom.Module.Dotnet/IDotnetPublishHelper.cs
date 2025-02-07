﻿namespace DecSm.Atom.Module.Dotnet;

[TargetDefinition]
public partial interface IDotnetPublishHelper : IVersionHelper
{
    async Task DotnetPublishProject(DotnetPublishOptions options)
    {
        Logger.LogInformation("Publishing Atom project {AtomProjectName}", options.ProjectName);

        var projectPath = DotnetFileUtils.GetProjectFilePathByName(FileSystem, options.ProjectName);

        await using var transformFilesScope = (options.AutoSetVersion, options.CustomPropertiesTransform) switch
        {
            (true, not null) => await TransformProjectVersionScope
                .CreateAsync(DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                    GetService<IBuildVersionProvider>()
                        .Version)
                .AddAsync(options.CustomPropertiesTransform),

            (true, null) => await TransformProjectVersionScope.CreateAsync(
                DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                GetService<IBuildVersionProvider>()
                    .Version),

            (false, not null) => await TransformMultiFileScope.CreateAsync(
                DotnetFileUtils.GetPropertyFilesForProject(projectPath, FileSystem.AtomRootDirectory),
                options.CustomPropertiesTransform!),

            _ => null,
        };

        var buildDir = FileSystem.AtomRootDirectory / options.ProjectName / "bin" / options.ProjectName;

        if (FileSystem.Directory.Exists(buildDir))
            FileSystem.Directory.Delete(buildDir, true);

        await GetService<ProcessRunner>()
            .RunAsync(new("dotnet", $"publish {projectPath.Path} -c {options.Configuration} -o {buildDir}"));

        var outputArtifactName = options.OutputArtifactName ?? options.ProjectName;
        var publishDir = FileSystem.AtomPublishDirectory / outputArtifactName;

        Logger.LogInformation("Moving publish directory {OutputDir} to {PublishDir}", buildDir, publishDir);

        if (FileSystem.Directory.Exists(publishDir))
            FileSystem.Directory.Delete(publishDir, true);

        if (!FileSystem.Directory.Exists(FileSystem.AtomPublishDirectory))
            FileSystem.Directory.CreateDirectory(FileSystem.AtomPublishDirectory);

        FileSystem.Directory.Move(buildDir, publishDir);

        Logger.LogInformation("Publishing Atom project {AtomProjectName} to {OutputArtifactName} completed",
            options.ProjectName,
            outputArtifactName);
    }
}
