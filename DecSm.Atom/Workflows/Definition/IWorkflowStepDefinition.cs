namespace DecSm.Atom.Workflows.Definition;

public interface IWorkflowStepDefinition
{
    IWorkflowStep CreateStep(IAtomBuild build);
};