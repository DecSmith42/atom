namespace DecSm.Atom.Workflows.Definition;

public interface IWorkflowStepDefinition
{
    IWorkflowStepModel CreateStep(IBuildDefinition buildDefinition);
}