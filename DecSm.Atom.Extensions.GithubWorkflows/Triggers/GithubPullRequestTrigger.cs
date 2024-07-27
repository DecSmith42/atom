namespace DecSm.Atom.Extensions.GithubWorkflows.Triggers;

public sealed record GithubPullRequestTrigger(IReadOnlyList<string> IncludedBranches, IReadOnlyList<string>? ExcludedBranches = null)
    : IWorkflowTrigger;