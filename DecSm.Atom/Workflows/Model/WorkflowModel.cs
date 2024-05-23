namespace DecSm.Atom.Workflows.Model;

public sealed record WorkflowModel(
    string Name,
    IReadOnlyList<IWorkflowTrigger> Triggers,
    IReadOnlyList<IWorkflowOption> Options,
    IReadOnlyList<WorkflowJobModel> Jobs
);