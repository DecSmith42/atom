namespace DecSm.Atom.Workflows.Model;

public sealed record WorkflowModel(string Name)
{
    public required IReadOnlyList<IWorkflowTrigger> Triggers { get; init; }

    public required IReadOnlyList<IWorkflowOption> Options { get; init; }

    public required IReadOnlyList<WorkflowJobModel> Jobs { get; init; }
}
