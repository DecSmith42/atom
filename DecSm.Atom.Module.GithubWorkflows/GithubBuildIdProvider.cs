namespace DecSm.Atom.Module.GithubWorkflows;

internal sealed class GithubBuildIdProvider(IBuildDefinition buildDefinition) : IBuildIdProvider
{
    public string? BuildId =>
        ProvideGithubRunIdAsWorkflowId.IsEnabled(IWorkflowOption.GetOptionsForCurrentTarget(buildDefinition))
            ? Github.Variables.RunId
            : null;
}
