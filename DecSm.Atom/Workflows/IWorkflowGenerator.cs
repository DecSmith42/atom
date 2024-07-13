namespace DecSm.Atom.Workflows;

internal interface IWorkflowGenerator
{
    Task GenerateWorkflows();

    Task<bool> WorkflowsDirty();
}