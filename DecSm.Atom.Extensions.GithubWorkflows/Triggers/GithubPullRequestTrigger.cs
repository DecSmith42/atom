namespace DecSm.Atom.Extensions.GithubWorkflows.Triggers;

public sealed record GithubPullRequestTrigger : IWorkflowTrigger
{
    public IReadOnlyList<string> IncludedBranches { get; init; } = [];

    public IReadOnlyList<string> ExcludedBranches { get; init; } = [];

    public IReadOnlyList<string> IncludedPaths { get; init; } = [];

    public IReadOnlyList<string> ExcludedPaths { get; init; } = [];

    public IReadOnlyList<string> Types { get; init; } = [];
}