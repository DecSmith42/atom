namespace DecSm.Atom.Module.Dotnet;

[TargetDefinition]
public partial interface IDotnetPackHelper : IVersionHelper
{
    async Task DotnetPackProject(DotnetPackOptions options)
    {
        Logger.LogInformation("Packing Atom project {AtomProjectName}", options.ProjectName);

        var project = FileSystem.FileInfo.New(FileSystem.AtomRootDirectory / options.ProjectName / $"{options.ProjectName}.csproj");
        var projectPath = new AbsolutePath(FileSystem, project.FullName);

        if (!project.Exists)
            throw new InvalidOperationException($"Project file {project.FullName} does not exist.");

        List<AbsolutePath> filesToTransform = [projectPath];

        var dir = projectPath;

        do
        {
            dir = dir.Parent;

            if (dir is null)
                break;

            var file = dir / "Directory.Build.props";

            if (file.FileExists)
                filesToTransform.Add(file);
        } while (dir != FileSystem.AtomRootDirectory);

        var buildVersionProvider = GetService<IBuildVersionProvider>();

        await using var setVersionScope = options.AutoSetVersion
            ? TransformProjectVersionScope.Create(filesToTransform, buildVersionProvider.Version)
            : null;

        var packDirectory = FileSystem.AtomRootDirectory / options.ProjectName / "bin" / options.Configuration;

        if (FileSystem.Directory.Exists(packDirectory))
            FileSystem.Directory.Delete(packDirectory, true);

        await GetService<ProcessRunner>()
            .RunAsync(new("dotnet", $"pack {project.FullName}"));

        var packageName = FileSystem
            .Directory
            .GetFiles(FileSystem.AtomRootDirectory / options.ProjectName / "bin" / options.Configuration, $"{options.ProjectName}.*.nupkg")
            .OrderByDescending(x => x)
            .First();

        // Move package to publish directory
        var packagePath = FileSystem.AtomRootDirectory / options.ProjectName / "bin" / options.Configuration / $"{packageName}";

        if (!packagePath.FileExists)
            throw new InvalidOperationException($"Package {packagePath} does not exist.");

        var outputArtifactName = options.OutputArtifactName ?? options.ProjectName;
        var publishDir = FileSystem.AtomPublishDirectory / outputArtifactName;
        Logger.LogInformation("Moving package {PackagePath} to {PublishDir}", packagePath, publishDir / packagePath.FileName!);

        if (FileSystem.Directory.Exists(publishDir))
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
