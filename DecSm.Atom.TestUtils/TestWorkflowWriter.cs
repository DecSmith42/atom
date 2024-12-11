namespace DecSm.Atom.TestUtils;

[PublicAPI]
public class TestWorkflowWriter : IWorkflowWriter<TestWorkflowType>
{
    public bool IsDirty { get; init; }

    public List<WorkflowModel> GeneratedWorkflows { get; } = [];

    public Task Generate(WorkflowModel workflow)
    {
        GeneratedWorkflows.Add(workflow);

        return Task.CompletedTask;
    }

    public Task<bool> CheckForDirtyWorkflow(WorkflowModel workflow) =>
        Task.FromResult(IsDirty);
}
