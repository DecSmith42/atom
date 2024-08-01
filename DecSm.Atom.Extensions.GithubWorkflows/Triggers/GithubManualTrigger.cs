namespace DecSm.Atom.Extensions.GithubWorkflows.Triggers;

public sealed record GithubManualTrigger(IReadOnlyList<ManualInput>? Inputs = null) : IWorkflowTrigger;