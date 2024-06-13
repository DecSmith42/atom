namespace Atom.Helpers;

[TargetDefinition]
public partial interface INugetHelper : IProcessHelper
{
    async Task PushPackageToNuget(AbsolutePath packagePath, string feed, string apiKey)
    {
        Logger.LogInformation("Pushing package to Nuget: {PackagePath}", packagePath);

        Logger.LogInformation("[Dry run] nuget push \"{PackagePath}\" --source {Feed} --api-key ***", packagePath, feed);

        // await RunProcess("dotnet", $"nuget push \"{packagePath}\" --source {feed} --api-key {apiKey}");
        // Logger.LogInformation("Package pushed");
    }
}