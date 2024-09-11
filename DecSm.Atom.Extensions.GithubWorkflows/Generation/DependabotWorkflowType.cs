namespace DecSm.Atom.Extensions.GithubWorkflows.Generation;

public sealed record DependabotWorkflowType : IWorkflowType
{
    public bool IsRunning => !string.IsNullOrWhiteSpace(Github.Variables.Workflow);
}
