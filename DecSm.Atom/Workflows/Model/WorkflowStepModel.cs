namespace DecSm.Atom.Workflows.Model;

/// <summary>
///     Represents a single step within a workflow.
///     This model defines the configuration for a specific operation or task in the workflow process.
/// </summary>
/// <param name="Name">The unique name or identifier for this workflow step.</param>
[PublicAPI]
public sealed record WorkflowStepModel(string Name)
{
    /// <summary>
    ///     Gets or initializes a value indicating whether artifact publishing should be suppressed for this step.
    ///     If set to <c>true</c>, any artifacts produced by this step will not be published.
    ///     Defaults to <c>false</c>.
    /// </summary>
    public bool SuppressArtifactPublishing { get; init; }

    /// <summary>
    ///     Gets or initializes the list of matrix dimensions for this step.
    ///     A matrix allows a step to be executed multiple times with different configurations,
    ///     where each dimension represents a variable in the configuration matrix.
    ///     Defaults to an empty list if not specified.
    /// </summary>
    /// <remarks>
    ///     For example, a step might be run against different versions of a dependency or target platforms.
    /// </remarks>
    public IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; } = [];

    /// <summary>
    ///     Gets or initializes the list of specific options or configurations for this workflow step.
    ///     These options can control various aspects of the step's execution.
    ///     Defaults to an empty list if not specified.
    /// </summary>
    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];
}
