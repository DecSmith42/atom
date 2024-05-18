namespace DecSm.Atom.Workflows.Model;

public sealed record WorkflowJob(string Name, IReadOnlyList<IWorkflowStep> Steps)
{
    public List<ParamDefinition> ParamRequirements { get; } = [];
    
    public List<WorkflowJob> JobRequirements { get; } = [];
}