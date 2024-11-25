namespace DecSm.Atom.Workflows;

[PublicAPI]
public interface IWorkflowOptionProvider
{
    public IReadOnlyList<IWorkflowOption> WorkflowOptions { get; }
}
