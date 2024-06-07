namespace DecSm.Atom.Workflows.Model;

public sealed record WorkflowJobModel(string Name, IReadOnlyList<IWorkflowStepModel> Steps)
{
    public List<ParamDefinition> ParamRequirements { get; init; } = [];
    
    public List<string> JobDependencies { get; init; } = [];
}