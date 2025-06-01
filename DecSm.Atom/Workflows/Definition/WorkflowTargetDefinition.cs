namespace DecSm.Atom.Workflows.Definition;

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

    public IWorkflowStepModel CreateModel() =>
        new WorkflowStepModel(Name)
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
