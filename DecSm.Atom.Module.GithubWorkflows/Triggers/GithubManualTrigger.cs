namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

public sealed record GithubManualTrigger(IReadOnlyList<ManualInput>? Inputs = null) : IWorkflowTrigger;
