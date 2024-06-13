namespace Atom.Targets;

[TargetDefinition]
public partial interface IPushToNuget : INugetHelper, IDotnetVersionHelper
{
    [ParamDefinition("nuget-feed", "The Nuget feed to push to.", "https://api.nuget.org/v3/index.json")]
    string NugetFeed => GetParam(() => NugetFeed) ?? "https://api.nuget.org/v3/index.json";

    [SecretDefinition("nuget-api-key", "The API key to use to push to Nuget.")]
    string? NugetApiKey => GetParam(() => NugetApiKey);

    Target PushToNuget =>
        d => d
            .ConsumesArtifact<IPackAtom>(IPackAtom.AtomProjectName)
            .ConsumesArtifact<IPackAtomGithubWorkflows>(IPackAtomGithubWorkflows.AtomGithubWorkflowsProjectName)
            .ConsumesArtifact<IPackAtomSourceGenerators>(IPackAtomSourceGenerators.AtomSourceGeneratorsProjectName)
            .ConsumesArtifact<IPackAtomTool>(IPackAtomTool.AtomToolProjectName)
            .RequiresParam(Build.Params.NugetFeed)
            .RequiresParam(Build.Secrets.NugetApiKey)
            .Executes(async () =>
            {
                await PushProject(IPackAtom.AtomProjectName);
                await PushProject(IPackAtomSourceGenerators.AtomSourceGeneratorsProjectName);
                await PushProject(IPackAtomGithubWorkflows.AtomGithubWorkflowsProjectName);
                await PushProject(IPackAtomTool.AtomToolProjectName);
            });

    private async Task PushProject(string projectName)
    {
        var packageBuildDir = FileSystem.ArtifactDirectory() / projectName;
        var packages = FileSystem.Directory.GetFiles(packageBuildDir, "*.nupkg");
        Logger.LogInformation("Found {PackagesLength} packages to push for {ProjectName}", packages.Length, projectName);

        var version = GetProjectPackageVersion(FileSystem.SolutionRoot() / projectName / $"{projectName}.csproj");
        Logger.LogInformation("Using package version {Version} for {ProjectName}", version, projectName);
        var matchingPackage = packages.Single(x => x == packageBuildDir / $"{projectName}.{version}.nupkg");

        await PushPackageToNuget(packageBuildDir / matchingPackage, NugetFeed, NugetApiKey!);
    }
}