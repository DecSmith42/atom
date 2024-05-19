namespace DecSm.Atom.GithubWorkflows.Triggers;

public sealed record GithubManualTrigger(IReadOnlyList<ManualInput>? Inputs = null) : IWorkflowTrigger;