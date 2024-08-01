namespace DecSm.Atom.Extensions.Dotnet.Helpers;

[TargetDefinition]
public partial interface IDotnetPackHelper : IVersionHelper
{
    async Task DotnetPackProject(string projectName)
    {
        Logger.LogInformation("Packing Atom project {AtomProjectName}", projectName);

        var project = FileSystem.FileInfo.New(FileSystem.SolutionRoot() / projectName / $"{projectName}.csproj");
        var projectPath = new AbsolutePath(FileSystem, project.FullName);

        if (!project.Exists)
            throw new InvalidOperationException($"Project file {project.FullName} does not exist.");

        MsBuildUtil.SetVersionInfo(projectPath,
            GetService<IBuildVersionProvider>()
                .Version);

        var packDirectory = FileSystem.SolutionRoot() / projectName / "bin" / "Release";

        if (FileSystem.Directory.Exists(packDirectory))
            FileSystem.Directory.Delete(packDirectory, true);

        await GetService<IProcessRunner>()
            .RunAsync(new("dotnet", $"pack {project.FullName}"));

        var packageName = FileSystem
            .Directory
            .GetFiles(FileSystem.SolutionRoot() / projectName / "bin" / "Release", $"{projectName}.*.nupkg")
            .OrderByDescending(x => x)
            .First();

        // Move package to publish directory
        var packagePath = FileSystem.SolutionRoot() / projectName / "bin" / "Release" / $"{packageName}";

        if (!packagePath.FileExists)
            throw new InvalidOperationException($"Package {packagePath} does not exist.");

        var publishDir = FileSystem.PublishDirectory() / projectName;
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

        Logger.LogInformation("Packed Atom project {AtomProjectName}", projectName);
    }
}