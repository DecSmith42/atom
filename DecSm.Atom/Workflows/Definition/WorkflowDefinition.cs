namespace DecSm.Atom.Workflows.Definition;

public sealed record WorkflowDefinition(string Name)
{
    public IReadOnlyList<IWorkflowStepDefinition> StepDefinitions { get; init; } = [];
    
    public IReadOnlyList<IWorkflowType> WorkflowTypes { get; init; } = [];
    
    public IReadOnlyList<IWorkflowTrigger> Triggers { get; init; } = [];
}

public interface IWorkflowTrigger;