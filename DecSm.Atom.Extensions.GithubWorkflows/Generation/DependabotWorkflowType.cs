namespace DecSm.Atom.Extensions.GithubWorkflows.Generation;

public record struct DependabotWorkflowType : IWorkflowType
{
    public bool IsRunning => Github.Variables.Actions.ToLower() is "true";
}