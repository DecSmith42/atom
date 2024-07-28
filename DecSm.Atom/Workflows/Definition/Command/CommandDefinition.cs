namespace DecSm.Atom.Workflows.Definition.Command;

[UsedImplicitly]
public sealed record CommandDefinition(string Name) : IWorkflowStepDefinition
{
    public IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; } = [];

    public bool SuppressArtifactPublishing { get; init; }

    public IWorkflowStepModel CreateStep(IBuildDefinition buildDefinition) =>
        new CommandWorkflowStep(Name)
        {
            MatrixDimensions = MatrixDimensions,
            SuppressArtifactPublishing = SuppressArtifactPublishing,
        };

    public CommandDefinition WithMatrixDimensions(params MatrixDimension[] dimensions) =>
        this with
        {
            MatrixDimensions = dimensions,
        };

    public CommandDefinition WithSuppressedArtifactPublishing =>
        this with
        {
            SuppressArtifactPublishing = true,
        };
}