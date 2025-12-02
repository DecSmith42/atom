namespace DecSm.Atom.Module.Dotnet.Helpers;

[PublicAPI]
public interface INugetHelper : IBuildInfo
{
    [ParamDefinition("nuget-dry-run", "Whether to perform a dry run of nuget write operations.")]
    bool NugetDryRun => GetParam(() => NugetDryRun);

    RootedPath NugetConfigPath
    {
        get
        {
            // Windows: %APPDATA%\NuGet\NuGet.Config
            // Linux: $HOME/.nuget/NuGet.Config
            // Mac: $HOME/.nuget/NuGet.Config
            var appDataPath =
                FileSystem.CreateRootedPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            return Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => appDataPath / "NuGet" / "NuGet.Config",
                _ => appDataPath / ".nuget" / "NuGet.Config",
            };
        }
    }

    /// <summary>
    ///     Pushes a project to a NuGet feed.
    /// </summary>
    /// <param name="projectName">The name of the project to push.</param>
    /// <param name="feed">The NuGet feed URL to push the project to.</param>
    /// <param name="apiKey">The API key for the NuGet feed.</param>
    /// <param name="configFile">Optional path to a NuGet configuration file to use instead of the default one.</param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    ///     Requires the <see cref="IBuildInfo.BuildVersion" /> param, which should be marked as consumed by the target that
    ///     calls this method.
    ///     <br /><br />
    ///     Requires the project to have been built and the package to be available in the artifacts directory.
    ///     <br /><br />
    ///     Requires the `dotnet` CLI to be installed and available in the PATH.
    /// </remarks>
    async Task PushProject(
        string projectName,
        string feed,
        string apiKey,
        RootedPath? configFile = null,
        CancellationToken cancellationToken = default)
    {
        var packageBuildDir = FileSystem.AtomArtifactsDirectory / projectName;
        var packages = FileSystem.Directory.GetFiles(packageBuildDir, "*.nupkg");

        if (packages.Length == 0)
        {
            Logger.LogWarning("No packages found in {PackageBuildDir}", packageBuildDir);

            return;
        }

        var matchingPackage = packages.Single(x => x == packageBuildDir / $"{projectName}.{BuildVersion}.nupkg");

        await PushPackageToNuget(packageBuildDir / matchingPackage, feed, apiKey, configFile, cancellationToken);
    }

    async Task PushPackageToNuget(
        RootedPath packagePath,
        string feed,
        string apiKey,
        RootedPath? configFile = null,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Pushing package to Nuget: {PackagePath}", packagePath);

        if (NugetDryRun)
        {
            Logger.LogInformation("Dry run: skipping nuget push \"{PackagePath}\" --soure {Feed} --api-key ***",
                packagePath,
                feed);

            return;
        }

        var configFileFlag = configFile is not null
            ? $" --configfile \"{configFile}\""
            : string.Empty;

        var processRunResult = await ProcessRunner.RunAsync(new("dotnet",
                $"nuget push \"{packagePath}\"{configFileFlag} --source {feed} --api-key {apiKey}"),
            cancellationToken);

        if (processRunResult.ExitCode is not 0)
            Logger.LogError("Failed to push package to Nuget: {ProcessRunResult}", processRunResult.Error);

        Logger.LogInformation("Package pushed");
    }

    async Task<bool> IsNugetPackageVersionPublished(
        string projectName,
        string version,
        string feedUrl,
        string? feedKey = null,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();

        if (feedKey is { Length: > 0 })
            httpClient.DefaultRequestHeaders.Add("X-NuGet-ApiKey", feedKey);

        var index = await httpClient.GetStringAsync(feedUrl, cancellationToken);

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

        var response = await httpClient.GetAsync(packageUrl, cancellationToken);

        if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.NotFound))
            throw new InvalidOperationException(
                $"Failed to check if version {version} of {projectName} is published: {response.StatusCode}");

        return response.StatusCode is HttpStatusCode.OK;
    }

    async Task<TransformFileScope> CreateNugetConfigOverwriteScope(
        string contents,
        CancellationToken cancellationToken = default) =>
        await TransformFileScope.CreateAsync(NugetConfigPath, _ => contents, cancellationToken);

    async Task CreateNugetConfigOverwriteScope(
        NugetFeed[] feeds,
        bool skipIfExists = false,
        CancellationToken cancellationToken = default)
    {
        if (skipIfExists)
        {
            var nugetContents = await FileSystem.File.ReadAllTextAsync(NugetConfigPath, cancellationToken);

            if (feeds.All(f => nugetContents.Contains(f.Url, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.LogInformation("Nuget config already contains package sources, skipping");

                return;
            }
        }

        var sources = string.Join(Environment.NewLine,
            feeds.Select((x, i) => $"<add key=\"{x.Name ?? $"Feed{i}"}\" value=\"{x.Url}\" />"));

        var credentials = string.Join(Environment.NewLine,
            feeds.Select((x, i) =>
            {
                var result = new StringBuilder();

                result.AppendLine($"    <{x.Name ?? $"Feed{i}"}>");

                var username = x.Username();

                if (username is not null)
                    result.AppendLine($"      <add key=\"Username\" value=\"{username}\" />");

                var password = x.Password();

                if (password is not null)
                    result.AppendLine($"      <add key=\"Password\" value=\"{password}\" />");

                var plainTextPassword = x.PlainTextPassword();

                if (plainTextPassword is not null)
                    result.AppendLine($"      <add key=\"ClearTextPassword\" value=\"{plainTextPassword}\" />");

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

        await CreateNugetConfigOverwriteScope(configText, cancellationToken);
    }
}
