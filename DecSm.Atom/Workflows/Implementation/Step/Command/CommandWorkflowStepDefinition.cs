namespace DecSm.Atom.Workflows.Implementation.Step.Command;

public sealed record CommandWorkflowStepDefinition(string Name) : IWorkflowStepDefinition
{
    public IWorkflowStep CreateStep(IAtomBuildDefinition buildDefinition) =>
        new CommandWorkflowStep(Name);
}