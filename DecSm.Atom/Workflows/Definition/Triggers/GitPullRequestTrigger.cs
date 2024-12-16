namespace DecSm.Atom.Workflows.Definition.Triggers;

[PublicAPI]
public sealed record GitPullRequestTrigger : IWorkflowTrigger
{
    public IReadOnlyList<string> IncludedBranches { get; init; } = [];

    public IReadOnlyList<string> ExcludedBranches { get; init; } = [];

    public IReadOnlyList<string> IncludedPaths { get; init; } = [];

    public IReadOnlyList<string> ExcludedPaths { get; init; } = [];

    public IReadOnlyList<string> Types { get; init; } = [];

    public static GitPullRequestTrigger IntoMain { get; } = new()
    {
        IncludedBranches = ["main"],
    };
}
