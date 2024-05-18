namespace DecSm.Atom.Workflows.Implementation.Step.Command;

public sealed record CommandWorkflowStepDefinition(string Name) : IWorkflowStepDefinition
{
    public IWorkflowStep CreateStep(IAtomBuild build) =>
        new CommandWorkflowStep(Name);
}