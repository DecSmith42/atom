namespace DecSm.Atom.GithubWorkflows.Generation;

public readonly record struct GithubWorkflowType : IWorkflowType
{
    public bool IsRunning => Github.Variables.Actions.ToLower() is "true";
}