namespace Atom.Helpers;

[TargetDefinition]
public partial interface INugetHelper : IProcessHelper, IDotnetVersionHelper
{
    async Task PushProject(string projectName, string feed, string apiKey)
    {
        var packageBuildDir = FileSystem.ArtifactDirectory() / projectName;
        var packages = FileSystem.Directory.GetFiles(packageBuildDir, "*.nupkg");

        if (packages.Length == 0)
        {
            Logger.LogWarning("No packages found in {PackageBuildDir}", packageBuildDir);

            return;
        }

        var version = GetProjectPackageVersion(FileSystem.SolutionRoot() / projectName / $"{projectName}.csproj");
        var matchingPackage = packages.Single(x => x == packageBuildDir / $"{projectName}.{version}.nupkg");

        await PushPackageToNuget(packageBuildDir / matchingPackage, feed, apiKey!);
    }

    async Task PushPackageToNuget(AbsolutePath packagePath, string feed, string apiKey)
    {
        Logger.LogInformation("Pushing package to Nuget: {PackagePath}", packagePath);

        await RunProcess("dotnet", $"nuget push \"{packagePath}\" --source {feed} --api-key {apiKey}");
        Logger.LogInformation("Package pushed");
    }
}