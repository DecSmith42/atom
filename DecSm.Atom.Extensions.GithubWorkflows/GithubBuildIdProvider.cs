namespace DecSm.Atom.Extensions.GithubWorkflows;

public sealed class GithubBuildIdProvider(IBuildDefinition buildDefinition) : IBuildIdProvider
{
    public string? BuildId =>
        ProvideGithubRunIdAsWorkflowId.IsEnabled(WorkflowOptionUtil.GetOptionsForCurrentTarget(buildDefinition))
            ? Github.Variables.RunId
            : null;
}
