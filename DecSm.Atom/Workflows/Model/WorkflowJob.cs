namespace DecSm.Atom.Workflows.Model;

public sealed record WorkflowJob(string Name, IReadOnlyList<IWorkflowStep> Steps);