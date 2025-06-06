namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a workflow trigger that activates on Git pull request events.
///     Provides fine-grained control over trigger conditions through branch patterns,
///     file path filters, and pull request type specifications.
/// </summary>
/// <remarks>
///     This trigger supports both inclusive and exclusive filtering for branches and paths.
///     When multiple filters are specified, they work together to determine if the trigger
///     should activate. Empty collections are treated as "no filter applied".
/// </remarks>
/// <example>
///     <code>
///     [BuildDefinition]
///     partial class Build : BuildDefinition, ISomeWorkflowIntegration
///     {
///         public override IReadOnlyList&lt;WorkflowDefinition&gt; Workflows =>
///         [
///             new("SomeWorkflow")
///             {
///                 Triggers = [GitPullRequestTrigger.IntoMain],
///                 Targets = ...,
///                 WorkflowTypes = ...,
///             },
///         ];
///     }
///     </code>
/// </example>
[PublicAPI]
public sealed record GitPullRequestTrigger : IWorkflowTrigger
{
    /// <summary>
    ///     Gets the list of branch patterns that should trigger the workflow.
    ///     If empty, all branches are included unless explicitly excluded.
    ///     Supports glob patterns for flexible branch matching.
    /// </summary>
    /// <value>A read-only list of branch patterns to include. Defaults to an empty collection.</value>
    public IReadOnlyList<string> IncludedBranches { get; init; } = [];

    /// <summary>
    ///     Gets the list of branch patterns that should NOT trigger the workflow.
    ///     Takes precedence over <see cref="IncludedBranches" /> when there are conflicts.
    ///     Supports glob patterns for flexible branch matching.
    /// </summary>
    /// <value>A read-only list of branch patterns to exclude. Defaults to an empty collection.</value>
    public IReadOnlyList<string> ExcludedBranches { get; init; } = [];

    /// <summary>
    ///     Gets the list of file path patterns that must be affected for the trigger to activate.
    ///     If empty, all file changes are considered relevant unless explicitly excluded.
    ///     Supports glob patterns for flexible path matching.
    /// </summary>
    /// <value>A read-only list of file path patterns to include. Defaults to an empty collection.</value>
    public IReadOnlyList<string> IncludedPaths { get; init; } = [];

    /// <summary>
    ///     Gets the list of file path patterns that should NOT activate the trigger.
    ///     Takes precedence over <see cref="IncludedPaths" /> when there are conflicts.
    ///     Supports glob patterns for flexible path matching.
    /// </summary>
    /// <value>A read-only list of file path patterns to exclude. Defaults to an empty collection.</value>
    public IReadOnlyList<string> ExcludedPaths { get; init; } = [];

    /// <summary>
    ///     Gets the list of pull request event types that should activate this trigger.
    ///     Common types include "opened", "closed", "synchronize", "reopened".
    ///     If empty, all pull request event types will activate the trigger.
    /// </summary>
    /// <value>A read-only list of pull request event types. Defaults to an empty collection.</value>
    public IReadOnlyList<string> Types { get; init; } = [];

    /// <summary>
    ///     Gets a pre-configured trigger instance that activates on pull requests targeting the main branch.
    ///     This is a convenience property for the most common pull request trigger scenario.
    /// </summary>
    /// <value>A <see cref="GitPullRequestTrigger" /> configured to trigger on pull requests into the main branch.</value>
    public static GitPullRequestTrigger IntoMain { get; } = new()
    {
        IncludedBranches = ["main"],
    };
}
