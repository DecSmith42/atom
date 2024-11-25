namespace DecSm.Atom.Workflows.Definition;

[PublicAPI]
public interface IWorkflowStepDefinition
{
    IWorkflowStepModel CreateStep();
}
