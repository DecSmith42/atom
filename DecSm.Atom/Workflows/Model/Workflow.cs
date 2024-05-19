namespace DecSm.Atom.Workflows.Model;

public sealed record Workflow(
    string Name,
    IReadOnlyList<IWorkflowTrigger> Triggers,
    IReadOnlyList<IWorkflowOption> Options,
    IReadOnlyList<WorkflowJob> Jobs
);