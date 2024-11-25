namespace DecSm.Atom.Module.GithubWorkflows.Generation;

[PublicAPI]
public sealed record GithubWorkflowType : IWorkflowType
{
    public bool IsRunning => Github.IsGithubActions;
}
