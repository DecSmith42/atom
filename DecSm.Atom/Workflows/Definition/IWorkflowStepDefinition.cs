namespace DecSm.Atom.Workflows.Definition;

public interface IWorkflowStepDefinition
{
    IWorkflowStep CreateStep(IAtomBuildDefinition buildDefinition);
}