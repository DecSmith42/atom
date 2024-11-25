namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

[PublicAPI]
public sealed record GithubManualTrigger(IReadOnlyList<ManualInput>? Inputs = null) : IWorkflowTrigger;
