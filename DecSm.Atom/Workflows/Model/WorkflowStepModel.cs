namespace DecSm.Atom.Workflows.Model;

[PublicAPI]
public sealed record WorkflowStepModel(string Name) : IWorkflowStepModel
{
    public bool SuppressArtifactPublishing { get; init; }

    public IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; } = [];

    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];
}
