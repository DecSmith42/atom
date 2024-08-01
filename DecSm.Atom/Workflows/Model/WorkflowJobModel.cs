namespace DecSm.Atom.Workflows.Model;

public sealed record WorkflowJobModel(string Name, IReadOnlyList<IWorkflowStepModel> Steps)
{
    public List<string> JobDependencies { get; } = [];

    public IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; } = [];
}