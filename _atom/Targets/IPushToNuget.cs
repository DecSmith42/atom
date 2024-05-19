namespace Atom.Targets;

[TargetDefinition]
public partial interface IPushToNuget : INugetHelper
{
    [Param("nuget-feed", "The Nuget feed to push to.")]
    string? NugetFeed => GetParam(() => NugetFeed);
    
    [SecretParam("nuget-api-key", "The API key to use to push to Nuget.")]
    string? NugetApiKey => GetParam(() => NugetApiKey);
    
    Target PushToNuget =>
        d => d
            .DependsOn<IPackAtom>()
            .DependsOn<IPackAtomSourceGenerators>()
            .Requires(() => NugetFeed)
            .Requires(() => NugetApiKey)
            .Executes(async () =>
            {
                await PushProject(IPackAtom.AtomProjectName);
                await PushProject(IPackAtomSourceGenerators.AtomSourceGeneratorsProjectName);
            });
    
    private async Task PushProject(string projectName)
    {
        var packageBuildDir = FileSystem.AtomRoot() / projectName / "bin" / "Release";
        var packages = FileSystem.Directory.GetFiles(packageBuildDir, "*.nupkg");
        var version = GetProjectPackageVersion(FileSystem.AtomRoot() / projectName / $"{projectName}.csproj");
        var matchingPackage = packages.Single(x => x == packageBuildDir / $"{projectName}.{version}.nupkg");
        
        await PushPackageToNuget(packageBuildDir / matchingPackage, NugetFeed!, NugetApiKey!);
    }
    
    private string GetProjectPackageVersion(AbsolutePath project)
    {
        List<AbsolutePath> projectPropertyFiles = [project];
        
        var path = project.Parent;
        
        while (path?.Parent != null)
        {
            projectPropertyFiles.AddRange(FileSystem
                .Directory
                .GetFiles(path, "Directory.Build.props")
                .Select(x => path / x));
            
            if (path == FileSystem.AtomRoot())
                break;
            
            path = path.Parent;
        }
        
        foreach (var projectPropertyFile in projectPropertyFiles)
        {
            var text = FileSystem.File.ReadAllText(projectPropertyFile);
            
            // First, look for PackageVersion
            var match = Regex.Match(text, "<PackageVersion>(.*)</PackageVersion>");
            
            if (match is { Success: true, Groups.Count: > 1 } && match.Groups[1].Value != "$(Version)")
                return match.Groups[1].Value;
            
            // If not found, look for Version
            match = Regex.Match(text, "<Version>(.*)</Version>");
            
            if (match is { Success: true, Groups.Count: > 1 })
                return match.Groups[1].Value;
        }
        
        throw new InvalidOperationException("Could not find PackageVersion in .csproj or any Directory.Build.props file");
    }
}