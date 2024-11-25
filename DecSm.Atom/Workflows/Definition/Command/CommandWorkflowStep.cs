namespace DecSm.Atom.Workflows.Definition.Command;

[PublicAPI]
public sealed record CommandWorkflowStep(string Name) : IWorkflowStepModel
{
    public bool SuppressArtifactPublishing { get; init; }

    public IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; } = [];

    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];
}
