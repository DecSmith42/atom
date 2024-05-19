namespace DecSm.Atom.GithubWorkflows.Triggers;

public sealed record GithubPullRequestTrigger(IReadOnlyList<string> IncludedBranches, IReadOnlyList<string>? ExcludedBranches = null)
    : IWorkflowTrigger;