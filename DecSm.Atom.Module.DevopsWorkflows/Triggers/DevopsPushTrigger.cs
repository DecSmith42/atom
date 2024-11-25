namespace DecSm.Atom.Module.DevopsWorkflows.Triggers;

[PublicAPI]
public sealed record DevopsPushTrigger : IWorkflowTrigger
{
    public IReadOnlyList<string> IncludedBranches { get; init; } = [];

    public IReadOnlyList<string> ExcludedBranches { get; init; } = [];

    public IReadOnlyList<string> IncludedPaths { get; init; } = [];

    public IReadOnlyList<string> ExcludedPaths { get; init; } = [];

    public IReadOnlyList<string> IncludedTags { get; init; } = [];

    public IReadOnlyList<string> ExcludedTags { get; init; } = [];
}
