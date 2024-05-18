namespace DecSm.Atom.Workflows.Model;

public sealed record Workflow(string Name, IReadOnlyList<WorkflowJob> Jobs);