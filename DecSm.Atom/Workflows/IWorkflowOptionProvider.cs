namespace DecSm.Atom.Workflows;

public interface IWorkflowOptionProvider
{
    public IReadOnlyList<IWorkflowOption> WorkflowOptions { get; }
}
