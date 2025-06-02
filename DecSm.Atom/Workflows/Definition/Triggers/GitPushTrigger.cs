namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a workflow trigger that activates when code is pushed to a Git repository.
///     Provides filtering capabilities for branches, file paths, and tags to control when the workflow should execute.
/// </summary>
/// <remarks>
///     <para>
///         This trigger supports multiple filtering mechanisms that can be combined:
///         - Branch filtering (include/exclude specific branches)
///         - Path filtering (include/exclude specific file paths)
///         - Tag filtering (include/exclude specific tags)
///     </para>
///     <para>
///         When filter lists are empty, no filtering is applied for that category (all items are allowed).
///         When both inclusion and exclusion filters are specified for the same category,
///         an item must match the inclusion criteria AND not match the exclusion criteria.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // Trigger on pushes to main branch only
/// var mainTrigger = GitPushTrigger.ToMain;
/// 
/// // Trigger on version tag pushes
/// var releaseTrigger = new GitPushTrigger
/// {
///     IncludedTags = ["v[0-9]+.[0-9]+.[0-9]+"]
/// };
/// 
/// // Trigger on feature branches, excluding documentation changes
/// var featureTrigger = new GitPushTrigger
/// {
///     IncludedBranches = ["feature/*"],
///     ExcludedPaths = ["docs/**", "*.md"]
/// };
/// </code>
/// </example>
[PublicAPI]
public sealed record GitPushTrigger : IWorkflowTrigger
{
    /// <summary>
    ///     Gets or sets the list of branch patterns that should trigger the workflow.
    ///     When empty, all branches are included unless explicitly excluded.
    /// </summary>
    /// <value>A read-only list of branch name patterns. Supports wildcards and regex patterns.</value>
    public IReadOnlyList<string> IncludedBranches { get; init; } = [];

    /// <summary>
    ///     Gets or sets the list of branch patterns that should NOT trigger the workflow.
    ///     Takes precedence over <see cref="IncludedBranches" /> when both are specified.
    /// </summary>
    /// <value>A read-only list of branch name patterns to exclude. Supports wildcards and regex patterns.</value>
    public IReadOnlyList<string> ExcludedBranches { get; init; } = [];

    /// <summary>
    ///     Gets or sets the list of file path patterns that should trigger the workflow.
    ///     When empty, changes to all paths are included unless explicitly excluded.
    /// </summary>
    /// <value>A read-only list of file path patterns. Supports wildcards and glob patterns.</value>
    public IReadOnlyList<string> IncludedPaths { get; init; } = [];

    /// <summary>
    ///     Gets or sets the list of file path patterns that should NOT trigger the workflow.
    ///     Takes precedence over <see cref="IncludedPaths" /> when both are specified.
    /// </summary>
    /// <value>A read-only list of file path patterns to exclude. Supports wildcards and glob patterns.</value>
    public IReadOnlyList<string> ExcludedPaths { get; init; } = [];

    /// <summary>
    ///     Gets or sets the list of tag patterns that should trigger the workflow.
    ///     When empty, all tags are included unless explicitly excluded.
    /// </summary>
    /// <value>A read-only list of tag patterns. Supports wildcards and regex patterns.</value>
    public IReadOnlyList<string> IncludedTags { get; init; } = [];

    /// <summary>
    ///     Gets or sets the list of tag patterns that should NOT trigger the workflow.
    ///     Takes precedence over <see cref="IncludedTags" /> when both are specified.
    /// </summary>
    /// <value>A read-only list of tag patterns to exclude. Supports wildcards and regex patterns.</value>
    public IReadOnlyList<string> ExcludedTags { get; init; } = [];

    /// <summary>
    ///     Gets a predefined trigger that activates only on pushes to the main branch.
    ///     This is a commonly used trigger for continuous integration workflows.
    /// </summary>
    /// <value>A <see cref="GitPushTrigger" /> configured to include only the "main" branch.</value>
    public static GitPushTrigger ToMain { get; } = new()
    {
        IncludedBranches = ["main"],
    };
}
