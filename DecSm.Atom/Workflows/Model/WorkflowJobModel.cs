namespace DecSm.Atom.Workflows.Model;

[PublicAPI]
public sealed record WorkflowJobModel(string Name, IReadOnlyList<IWorkflowTargetModel> Steps)
{
    public required IReadOnlyList<string> JobDependencies { get; init; }

    public required IReadOnlyList<IWorkflowOption> Options { get; init; }

    public required IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; }
}
