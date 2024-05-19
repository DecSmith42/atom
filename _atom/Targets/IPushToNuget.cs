namespace Atom.Targets;

[TargetDefinition]
public partial interface IPushToNuget : INugetHelper, IVersionHelper
{
    [Param("nuget-feed", "The Nuget feed to push to.")]
    string? NugetFeed => GetParam(() => NugetFeed);
    
    [SecretParam("nuget-api-key", "The API key to use to push to Nuget.")]
    string? NugetApiKey => GetParam(() => NugetApiKey);
    
    Target PushToNuget =>
        d => d
            .Consumes<IPackAtom>(IPackAtom.AtomProjectName)
            .Consumes<IPackAtomSourceGenerators>(IPackAtomSourceGenerators.AtomSourceGeneratorsProjectName)
            .Requires(() => NugetFeed)
            .Requires(() => NugetApiKey)
            .Executes(async () =>
            {
                await PushProject(IPackAtom.AtomProjectName);
                await PushProject(IPackAtomSourceGenerators.AtomSourceGeneratorsProjectName);
            });
    
    private async Task PushProject(string projectName)
    {
        var packageBuildDir = FileSystem.ArtifactDirectory() / projectName;
        var packages = FileSystem.Directory.GetFiles(packageBuildDir, "*.nupkg");
        var version = GetProjectPackageVersion(FileSystem.SolutionRoot() / projectName / $"{projectName}.csproj");
        var matchingPackage = packages.Single(x => x == packageBuildDir / $"{projectName}.{version}.nupkg");
        
        await PushPackageToNuget(packageBuildDir / matchingPackage, NugetFeed!, NugetApiKey!);
    }
}