namespace DecSm.Atom.Workflows.Model;

public sealed record WorkflowJobModel(string Name, IReadOnlyList<IWorkflowStepModel> Steps)
{
    public required IReadOnlyList<string> JobDependencies { get; init; }

    public required IReadOnlyList<IWorkflowOption> Options { get; init; }

    public required IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; }
}
