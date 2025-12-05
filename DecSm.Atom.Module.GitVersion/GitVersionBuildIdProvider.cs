namespace DecSm.Atom.Module.GitVersion;

/// <summary>
///     Provides the build ID using GitVersion.
/// </summary>
/// <remarks>
///     This provider executes the GitVersion tool to extract the full semantic version,
///     which is then used as the build ID. It requires the GitVersion.Tool to be installed.
/// </remarks>
internal sealed class GitVersionBuildIdProvider(
    IDotnetToolInstallHelper dotnetToolInstallHelper,
    IProcessRunner processRunner,
    IBuildDefinition buildDefinition
) : IBuildIdProvider
{
    /// <summary>
    ///     Gets the build ID, derived from GitVersion's FullSemVer.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if GitVersion is not enabled via <see cref="UseGitVersionForBuildId" />
    ///     or if the build ID cannot be determined from GitVersion's output.
    /// </exception>
    [field: AllowNull]
    [field: MaybeNull]
    public string BuildId
    {
        get
        {
            if (!UseGitVersionForBuildId.IsEnabled(IWorkflowOption.GetOptionsForCurrentTarget(buildDefinition)))
                throw new InvalidOperationException(
                    "GitVersion is not enabled for build ID generation. Ensure UseGitVersionForBuildId is enabled.");

            if (field is { Length: > 0 })
                return field;

            dotnetToolInstallHelper.InstallTool("GitVersion.Tool");

            var gitVersionResult = processRunner.Run(new("dotnet", "gitversion /output json")
            {
                InvocationLogLevel = LogLevel.Debug,
            });

            var jsonOutput =
                JsonSerializer.Deserialize(gitVersionResult.Output, JsonElementContext.Default.JsonElement);

            var buildId = jsonOutput
                .GetProperty("FullSemVer")
                .GetString();

            return field = buildId ?? throw new InvalidOperationException("Failed to determine build ID");
        }
    }

    /// <summary>
    ///     Gets a group identifier for the given build ID, typically representing the major.minor.patch version.
    /// </summary>
    /// <param name="buildId">The build ID (expected to be a semantic version string).</param>
    /// <returns>A string representing the major.minor.patch group.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the build ID cannot be parsed as a semantic version.</exception>
    public string GetBuildIdGroup(string buildId) =>
        !SemVer.TryParse(buildId, out var version)
            ? throw new InvalidOperationException($"Failed to parse build ID '{buildId}' as a semantic version.")
            : $"{version.Major}.{version.Minor}.{version.Patch}";
}
