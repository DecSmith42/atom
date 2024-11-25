namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

[PublicAPI]
public sealed record GithubPullRequestTrigger : IWorkflowTrigger
{
    public IReadOnlyList<string> IncludedBranches { get; init; } = [];

    public IReadOnlyList<string> ExcludedBranches { get; init; } = [];

    public IReadOnlyList<string> IncludedPaths { get; init; } = [];

    public IReadOnlyList<string> ExcludedPaths { get; init; } = [];

    public IReadOnlyList<string> Types { get; init; } = [];
}
