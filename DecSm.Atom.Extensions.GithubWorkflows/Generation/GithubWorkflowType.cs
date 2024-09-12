namespace DecSm.Atom.Extensions.GithubWorkflows.Generation;

public sealed record GithubWorkflowType : IWorkflowType
{
    public bool IsRunning => !string.IsNullOrWhiteSpace(Github.Variables.Workflow);
}
