namespace DecSm.Atom.Workflows.Definition.Command;

public sealed record CommandDefinition(string Name) : IWorkflowStepDefinition
{
    public IWorkflowStepModel CreateStep(IBuildDefinition buildDefinition) =>
        new CommandWorkflowStep(Name);
}