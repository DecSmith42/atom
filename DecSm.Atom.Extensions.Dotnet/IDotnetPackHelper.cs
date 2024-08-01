namespace DecSm.Atom.Extensions.Dotnet;

[TargetDefinition]
public partial interface IDotnetPackHelper : IVersionHelper
{
    async Task DotnetPackProject(DotnetPackOptions options)
    {
        Logger.LogInformation("Packing Atom project {AtomProjectName}", options.ProjectName);

        var project = FileSystem.FileInfo.New(FileSystem.SolutionRoot() / options.ProjectName / $"{options.ProjectName}.csproj");
        var projectPath = new AbsolutePath(FileSystem, project.FullName);

        if (!project.Exists)
            throw new InvalidOperationException($"Project file {project.FullName} does not exist.");

        await using var setVersionScope = options.AutoSetVersion
            ? TransformProjectVersionScope.Create(projectPath,
                GetService<IBuildVersionProvider>()
                    .Version)
            : null;

        var packDirectory = FileSystem.SolutionRoot() / options.ProjectName / "bin" / options.Configuration;

        if (FileSystem.Directory.Exists(packDirectory))
            FileSystem.Directory.Delete(packDirectory, true);

        await GetService<IProcessRunner>()
            .RunAsync(new("dotnet", $"pack {project.FullName}"));

        var packageName = FileSystem
            .Directory
            .GetFiles(FileSystem.SolutionRoot() / options.ProjectName / "bin" / options.Configuration, $"{options.ProjectName}.*.nupkg")
            .OrderByDescending(x => x)
            .First();

        // Move package to publish directory
        var packagePath = FileSystem.SolutionRoot() / options.ProjectName / "bin" / options.Configuration / $"{packageName}";

        if (!packagePath.FileExists)
            throw new InvalidOperationException($"Package {packagePath} does not exist.");

        var publishDir = FileSystem.PublishDirectory() / options.ProjectName;
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