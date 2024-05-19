namespace DecSm.Atom.Workflows.Definition.Command;

public sealed record CommandDefinition(string Name) : IWorkflowStepDefinition
{
    public IWorkflowStep CreateStep(IAtomBuildDefinition buildDefinition) =>
        new CommandWorkflowStep(Name);
}