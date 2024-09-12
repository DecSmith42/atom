namespace DecSm.Atom.Extensions.GitVersion;

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
            if (!ProvideGitVersionAsWorkflowId.IsEnabled(WorkflowOptionUtil.GetOptionsForCurrentTarget(buildDefinition)))
                return null;

            if (_buildId is not null)
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
}
