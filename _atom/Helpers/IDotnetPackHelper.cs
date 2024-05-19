namespace Atom.Helpers;

[TargetDefinition]
public partial interface IDotnetPackHelper : IProcessHelper, IVersionHelper
{
    async Task DotnetPackProject(string projectName)
    {
        Logger.LogInformation("Packing Atom project {AtomProjectName}", projectName);
        
        var project = FileSystem.FileInfo.New(FileSystem.SolutionRoot() / projectName / $"{projectName}.csproj");
        
        if (!project.Exists)
            throw new InvalidOperationException($"Project file {project.FullName} does not exist.");
        
        var packageVersion = GetProjectPackageVersion(AbsolutePath.FromFileInfo(project));
        
        await RunProcess("dotnet", $"pack {project.FullName}");
        
        // Move package to publish directory
        var packagePath = FileSystem.SolutionRoot() / projectName / "bin" / "Release" / $"{projectName}.{packageVersion}.nupkg";
        var publishDir = FileSystem.PublishDirectory() / projectName;
        Logger.LogInformation("Moving package {PackagePath} to {PublishDir}", packagePath, publishDir / packagePath.FileName!);
        FileSystem.File.Move(packagePath, publishDir / packagePath.FileName!);
        
        Logger.LogInformation("Packed Atom project {AtomProjectName}", projectName);
    }
}