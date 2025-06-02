namespace DecSm.Atom.Workflows.Definition;

/// <summary>
///     Defines a single target or step within a workflow.
/// </summary>
/// <param name="Name">The name of the workflow target.</param>
/// <remarks>
///     <para>
///         A <c>WorkflowTargetDefinition</c> represents an individual unit of work within a larger <see cref="WorkflowDefinition" />.
///         It holds configuration specific to this target, such as matrix execution strategies, custom options,
///         and artifact publishing behavior.
///     </para>
///     <para>
///         Instances of <c>WorkflowTargetDefinition</c> are typically created and configured when defining the
///         <see cref="WorkflowDefinition.StepDefinitions" />
///         for a workflow.
///     </para>
/// </remarks>
[PublicAPI]
public sealed record WorkflowTargetDefinition(string Name)
{
    public IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; } = [];

    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];

    public bool SuppressArtifactPublishing { get; init; }

    public WorkflowTargetDefinition WithSuppressedArtifactPublishing =>
        this with
        {
            SuppressArtifactPublishing = true,
        };

    public WorkflowStepModel CreateModel() =>
        new(Name)
        {
            MatrixDimensions = MatrixDimensions,
            SuppressArtifactPublishing = SuppressArtifactPublishing,
            Options = Options,
        };

    public WorkflowTargetDefinition WithMatrixDimensions(params MatrixDimension[] dimensions) =>
        this with
        {
            MatrixDimensions = dimensions,
        };

    public WorkflowTargetDefinition WithAddedMatrixDimensions(params MatrixDimension[] dimensions) =>
        this with
        {
            MatrixDimensions = MatrixDimensions
                .Concat(dimensions)
                .ToList(),
        };

    public WorkflowTargetDefinition WithOptions(params IWorkflowOption[] options) =>
        this with
        {
            Options = options,
        };

    public WorkflowTargetDefinition WithAddedOptions(params IWorkflowOption[] options) =>
        this with
        {
            Options = Options
                .Concat(options)
                .ToList(),
        };
}
