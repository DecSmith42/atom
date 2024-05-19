namespace DecSm.Atom.GithubWorkflows.Generation;

public record struct DependabotWorkflowType() : IWorkflowType
{
    public Type WriterType { get; } = typeof(DependabotWorkflowWriter);
}