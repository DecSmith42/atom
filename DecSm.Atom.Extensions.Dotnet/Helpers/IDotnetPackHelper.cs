using Microsoft.Extensions.DependencyInjection;

namespace DecSm.Atom.Extensions.Dotnet.Helpers;

[TargetDefinition]
public partial interface IDotnetPackHelper : IProcessHelper, IDotnetVersionHelper
{
    async Task DotnetPackProject(string projectName)
    {
        Logger.LogInformation("Packing Atom project {AtomProjectName}", projectName);

        var project = FileSystem.FileInfo.New(FileSystem.SolutionRoot() / projectName / $"{projectName}.csproj");
        var projectPath = new AbsolutePath(FileSystem, project.FullName);

        if (!project.Exists)
            throw new InvalidOperationException($"Project file {project.FullName} does not exist.");

        var originalProjectFile = project.ReadAllText();

        try
        {
            MsBuildUtil.SetVersionInfo(projectPath,
                Services.GetRequiredService<IBuildVersionProvider>()
                    .Version);

            var packageVersion = GetProjectPackageVersion(AbsolutePath.FromFileInfo(project));

            await RunProcessAsync("dotnet", $"pack {project.FullName}");

            // Move package to publish directory
            var packagePath = FileSystem.SolutionRoot() / projectName / "bin" / "Release" / $"{projectName}.{packageVersion}.nupkg";
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
        finally
        {
            project.WriteAllText(originalProjectFile);
        }
    }
}