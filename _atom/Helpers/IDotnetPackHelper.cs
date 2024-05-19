namespace Atom.Helpers;

[TargetDefinition]
public partial interface IDotnetPackHelper : IProcessHelper
{
    async Task DotnetPackProject(string projectName)
    {
        Logger.LogInformation("Packing Atom project {AtomProjectName}", projectName);
        
        var fs = Services.GetRequiredService<IFileSystem>();
        var project = fs.FileInfo.New(fs.SolutionRoot() / projectName / $"{projectName}.csproj");
        
        if (!project.Exists)
            throw new InvalidOperationException($"Project file {project.FullName} does not exist.");
        
        await RunProcess("dotnet", $"pack {project.FullName}");
        
        // Copy package to publish directory
        // TODO
        
        Logger.LogInformation("Packed Atom project {AtomProjectName}", projectName);
    }
}