using System.Text;

namespace DecSm.Atom.Module.Dotnet;

[TargetDefinition]
public partial interface INugetHelper : IVersionHelper
{
    [ParamDefinition("nuget-dry-run", "Whether to perform a dry run of nuget write operations.", "false")]
    bool NugetDryRun => GetParam(() => NugetDryRun);

    async Task PushProject(string projectName, string feed, string apiKey, string? configFile = null)
    {
        var packageBuildDir = FileSystem.AtomArtifactsDirectory / projectName;
        var packages = FileSystem.Directory.GetFiles(packageBuildDir, "*.nupkg");

        if (packages.Length == 0)
        {
            Logger.LogWarning("No packages found in {PackageBuildDir}", packageBuildDir);

            return;
        }

        var matchingPackage = packages.Single(x => x == packageBuildDir / $"{projectName}.{Version}.nupkg");

        await PushPackageToNuget(packageBuildDir / matchingPackage, feed, apiKey);
    }

    async Task PushPackageToNuget(AbsolutePath packagePath, string feed, string apiKey, string? configFile = null)
    {
        Logger.LogInformation("Pushing package to Nuget: {PackagePath}", packagePath);

        if (NugetDryRun)
        {
            Logger.LogInformation("Dry run: skipping nuget push \"{PackagePath}\" --soure {Feed} --api-key ***", packagePath, feed);

            return;
        }

        await GetService<IProcessRunner>()
            .RunAsync(new("dotnet", $"nuget push \"{packagePath}\" --source {feed} --api-key {apiKey}"));

        Logger.LogInformation("Package pushed");
    }

    async Task<bool> IsNugetPackageVersionPublished(string projectName, string version, string feedUrl, string? feedKey = null)
    {
        using var httpClient = new HttpClient();

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

    async Task<ITransformFileScope> CreateNugetConfigOverwriteScope(string contents)
    {
        // Windows: %APPDATA%\NuGet\NuGet.Config
        // Linux: $HOME/.nuget/NuGet.Config
        // Mac: $HOME/.nuget/NuGet.Config
        var appDataPath = FileSystem.CreateAbsolutePath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        var nugetConfigPath = appDataPath / ".nuget" / "NuGet.Config";

        return await ITransformFileScope.CreateAsync(nugetConfigPath, _ => contents);
    }

    async Task CreateNugetConfigOverwriteScope(params NugetFeed[] feeds)
    {
        var sources = string.Join(Environment.NewLine, feeds.Select((x, i) => $"<add key=\"{x.Name ?? $"Feed{i}"}\" value=\"{x.Url}\" />"));

        var credentials = string.Join(Environment.NewLine,
            feeds.Select((x, i) =>
            {
                var result = new StringBuilder();

                result.AppendLine($"    <{x.Name ?? $"Feed{i}"}>");

                if (x.Username is not null)
                    result.AppendLine($"      <add key=\"Username\" value=\"{x.Username}\" />");

                if (x.Password is not null)
                    result.AppendLine($"      <add key=\"Password\" value=\"{x.Password}\" />");

                if (x.PlainTextPassword is not null)
                    result.AppendLine($"      <add key=\"ClearTextPassword\" value=\"{x.PlainTextPassword}\" />");

                result.AppendLine($"    </{x.Name ?? $"Feed{i}"}>");

                return result.ToString();
            }));

        var configText = $"""
                          <configuration>
                            <packageSources>
                          {sources}
                            </packageSources>
                          
                            <packageSourceCredentials>
                          {credentials}
                            </packageSourceCredentials>
                          </configuration>
                          """;

        await CreateNugetConfigOverwriteScope(configText);
    }
}
