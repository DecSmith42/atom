namespace DecSm.Atom.Workflows.Model;

public sealed record WorkflowJob(string Name, IReadOnlyList<IWorkflowStep> Steps)
{
    public List<ParamDefinition> ParamRequirements { get; init; } = [];
    
    public List<WorkflowJob> JobRequirements { get; init; } = [];
}