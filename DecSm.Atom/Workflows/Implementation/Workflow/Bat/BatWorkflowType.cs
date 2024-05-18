namespace DecSm.Atom.Workflows.Implementation.Workflow.Bat;

public record struct BatWorkflowType() : IWorkflowType
{
    public Type WriterType { get; } = typeof(BatWorkflowWriter);
}