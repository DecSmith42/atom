namespace DecSm.Atom.Workflows.Definition;

[PublicAPI]
public interface IWorkflowTargetDefinition
{
    IWorkflowTargetModel CreateModel();
}
