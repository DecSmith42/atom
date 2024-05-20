namespace DecSm.Atom.Workflows.Definition;

public sealed record WorkflowDefinition(string Name)
{
    public IReadOnlyList<IWorkflowTrigger> Triggers { get; init; } = [];
    
    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];
    
    public IReadOnlyList<IWorkflowStepDefinition> StepDefinitions { get; init; } = [];
    
    public IReadOnlyList<IWorkflowType> WorkflowTypes { get; init; } = [];
}