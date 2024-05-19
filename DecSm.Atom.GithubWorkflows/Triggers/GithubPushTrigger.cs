namespace DecSm.Atom.GithubWorkflows.Triggers;

public sealed record GithubPushTrigger(IReadOnlyList<string> IncludedBranches, IReadOnlyList<string>? ExcludedBranches = null)
    : IWorkflowTrigger;