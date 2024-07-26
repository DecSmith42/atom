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

    async Task<bool> IsNugetPackageVersionPublished(string projectName, string version, string feedUrl, string? feedKey = null)
    {
        var httpClient = new HttpClient();

        if (feedKey is { Length: > 0 })
            httpClient.DefaultRequestHeaders.Add("X-NuGet-ApiKey", feedKey);

        var index = await httpClient.GetStringAsync(feedUrl);

        // {
        //   ...
        //   "resources": [
        //     ...
        //     {
        //       "@id": "https://api.nuget.org/...",
        //       "@type": "RegistrationsBaseUrl/Versioned",
        //       ...
        //     },
        //     ...
        //   ],
        //   ...
        //   }
        // }

        var registrationsBaseUrl = JsonNode
            .Parse(index)?["resources"]
            ?.AsArray()
            .Select(x => x)
            .FirstOrDefault(x => x?["@type"]
                                     ?.ToString() ==
                                 "RegistrationsBaseUrl/Versioned")?["@id"]
            ?.ToString();

        if (registrationsBaseUrl is null)
            throw new InvalidOperationException("Could not find the registrations base URL in the Nuget feed.");

        var packageUrl = $"{registrationsBaseUrl}{projectName.ToLowerInvariant()}/{version}.json";

        var response = await httpClient.GetAsync(packageUrl);

        if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.NotFound))
            throw new InvalidOperationException(
                $"Failed to check if version {version} of {projectName} is published: {response.StatusCode}");

        return response.StatusCode is HttpStatusCode.OK;
    }
}