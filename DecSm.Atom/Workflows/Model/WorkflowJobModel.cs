namespace DecSm.Atom.Workflows.Model;

public sealed record WorkflowJobModel(string Name, IReadOnlyList<IWorkflowStepModel> Steps)
{
    public IReadOnlyList<string> JobDependencies { get; init; } = [];

    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];

    public IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; } = [];
}
