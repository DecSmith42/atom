namespace DecSm.Atom.GithubWorkflows.Generation;

public record struct GithubWorkflowType() : IWorkflowType
{
    public Type WriterType { get; } = typeof(GithubWorkflowWriter);
}