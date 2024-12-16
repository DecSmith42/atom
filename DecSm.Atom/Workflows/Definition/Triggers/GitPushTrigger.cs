namespace DecSm.Atom.Workflows.Definition.Triggers;

[PublicAPI]
public sealed record GitPushTrigger : IWorkflowTrigger
{
    public IReadOnlyList<string> IncludedBranches { get; init; } = [];

    public IReadOnlyList<string> ExcludedBranches { get; init; } = [];

    public IReadOnlyList<string> IncludedPaths { get; init; } = [];

    public IReadOnlyList<string> ExcludedPaths { get; init; } = [];

    public IReadOnlyList<string> IncludedTags { get; init; } = [];

    public IReadOnlyList<string> ExcludedTags { get; init; } = [];

    public static GitPushTrigger ToMain { get; } = new()
    {
        IncludedBranches = ["main"],
    };
}
