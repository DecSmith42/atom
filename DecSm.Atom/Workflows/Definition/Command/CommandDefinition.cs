namespace DecSm.Atom.Workflows.Definition.Command;

[PublicAPI]
public sealed record CommandDefinition(string Name) : IWorkflowStepDefinition
{
    public IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; } = [];

    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];

    public bool SuppressArtifactPublishing { get; init; }

    public CommandDefinition WithSuppressedArtifactPublishing =>
        this with
        {
            SuppressArtifactPublishing = true,
        };

    public IWorkflowStepModel CreateStep() =>
        new CommandWorkflowStep(Name)
        {
            MatrixDimensions = MatrixDimensions,
            SuppressArtifactPublishing = SuppressArtifactPublishing,
            Options = Options,
        };

    public CommandDefinition WithMatrixDimensions(params MatrixDimension[] dimensions) =>
        this with
        {
            MatrixDimensions = dimensions,
        };

    public CommandDefinition WithAddedMatrixDimensions(params MatrixDimension[] dimensions) =>
        this with
        {
            MatrixDimensions = MatrixDimensions
                .Concat(dimensions)
                .ToList(),
        };

    public CommandDefinition WithOptions(params IWorkflowOption[] options) =>
        this with
        {
            Options = options,
        };

    public CommandDefinition WithAddedOptions(params IWorkflowOption[] options) =>
        this with
        {
            Options = Options
                .Concat(options)
                .ToList(),
        };
}
