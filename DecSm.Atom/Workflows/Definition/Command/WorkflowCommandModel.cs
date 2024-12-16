namespace DecSm.Atom.Workflows.Definition.Command;

[PublicAPI]
public sealed record WorkflowCommandModel(string Name) : IWorkflowTargetModel
{
    public bool SuppressArtifactPublishing { get; init; }

    public IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; } = [];

    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];
}
