namespace DecSm.Atom.Module.GitVersion;

public sealed class GitVersionBuildIdProvider(
    IDotnetToolHelper dotnetToolHelper,
    IProcessRunner processRunner,
    IBuildDefinition buildDefinition
) : IBuildIdProvider
{
    private string? _buildId;

    public string? BuildId
    {
        get
        {
            if (!ProvideGitVersionAsWorkflowId.IsEnabled(IWorkflowOption.GetOptionsForCurrentTarget(buildDefinition)))
                return null;

            if (_buildId is { Length: > 0 })
                return _buildId;

            dotnetToolHelper.InstallTool("GitVersion.Tool");

            var gitVersionResult = processRunner.Run(new("dotnet", "gitversion /output json")
            {
                InvocationLogLevel = LogLevel.Debug,
            });

            var jsonOutput = JsonSerializer.Deserialize<JsonElement>(gitVersionResult.Output);

            var buildId = jsonOutput
                .GetProperty("FullSemVer")
                .GetString();

            return _buildId = buildId ?? throw new InvalidOperationException("Failed to determine build ID");
        }
    }

    public string GetBuildIdPathPrefix(string buildId)
    {
        if (!SemVer.TryParse(buildId, out var version))
            throw new InvalidOperationException($"Failed to parse build ID '{buildId}' as SemVer");

        return $"{version.Major}.{version.Minor}.{version.Patch}";
    }
}
