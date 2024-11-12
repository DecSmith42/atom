namespace DecSm.Atom.Module.GithubWorkflows.Generation;

public sealed record GithubWorkflowType : IWorkflowType
{
    public bool IsRunning => Github.IsGithubActions;
}
