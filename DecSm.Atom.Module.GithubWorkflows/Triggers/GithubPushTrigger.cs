namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

public sealed record GithubPushTrigger : IWorkflowTrigger
{
    public IReadOnlyList<string> IncludedBranches { get; init; } = [];

    public IReadOnlyList<string> ExcludedBranches { get; init; } = [];

    public IReadOnlyList<string> IncludedPaths { get; init; } = [];

    public IReadOnlyList<string> ExcludedPaths { get; init; } = [];

    public IReadOnlyList<string> IncludedTags { get; init; } = [];

    public IReadOnlyList<string> ExcludedTags { get; init; } = [];
}
