namespace DecSm.Atom.Extensions.GithubWorkflows;

public sealed class GithubBuildIdProvider(IBuildDefinition buildDefinition) : IBuildIdProvider
{
    public string? BuildId =>
        WorkflowOptionUtil
            .GetOptionsForCurrentTarget(buildDefinition)
            .Contains(ProvideGithubRunIdAsWorkflowId.Enabled)
            ? Github.Variables.RunId
            : null;
}
