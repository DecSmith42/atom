namespace DecSm.Atom.Module.GitVersion;

internal sealed class GitVersionBuildIdProvider(
    IDotnetToolInstallHelper dotnetToolInstallHelper,
    IProcessRunner processRunner,
    IBuildDefinition buildDefinition
) : IBuildIdProvider
{
    [field: AllowNull]
    [field: MaybeNull]
    public string BuildId
    {
        get
        {
            if (!UseGitVersionForBuildId.IsEnabled(IWorkflowOption.GetOptionsForCurrentTarget(buildDefinition)))
                throw new InvalidOperationException("GitVersion is not enabled");

            if (field is { Length: > 0 })
                return field;

            dotnetToolInstallHelper.InstallTool("GitVersion.Tool");

            var gitVersionResult = processRunner.Run(new("dotnet", "gitversion /output json")
            {
                InvocationLogLevel = LogLevel.Debug,
            });

            var jsonOutput = JsonSerializer.Deserialize<JsonElement>(gitVersionResult.Output);

            var buildId = jsonOutput
                .GetProperty("FullSemVer")
                .GetString();

            return field = buildId ?? throw new InvalidOperationException("Failed to determine build ID");
        }
    }

    public string GetBuildIdGroup(string buildId) =>
        !SemVer.TryParse(buildId, out var version)
            ? throw new InvalidOperationException($"Failed to parse build ID '{buildId}' as SemVer")
            : $"{version.Major}.{version.Minor}.{version.Patch}";
}
