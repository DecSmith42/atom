namespace DecSm.Atom.Workflows;

internal interface IWorkflowGenerator
{
    Task GenerateWorkflows();

    Task<bool> WorkflowsDirty();
}

internal sealed class WorkflowGenerator(
    IBuildDefinition buildDefinition,
    IEnumerable<IWorkflowWriter> writers,
    WorkflowResolver workflowResolver
) : IWorkflowGenerator
{
    private readonly List<IWorkflowWriter> _writers = writers.ToList();

    public async Task GenerateWorkflows()
    {
        var workflowDefinitions = ((BuildDefinition)buildDefinition).Workflows;

        var generationTasks = new List<Task>();

        foreach (var workflowDefinition in workflowDefinitions)
        foreach (var workflowType in workflowDefinition.WorkflowTypes)
        {
            var writer = _writers.FirstOrDefault(w => w.WorkflowType == workflowType.GetType());

            if (writer is null)
                continue;

            var workflow = workflowResolver.Resolve(workflowDefinition);
            var generateTask = writer.Generate(workflow);

            generationTasks.Add(generateTask);
        }

        await Task.WhenAll(generationTasks);
    }

    public async Task<bool> WorkflowsDirty()
    {
        var workflowDefinitions = ((BuildDefinition)buildDefinition).Workflows;

        var checkTasks = new List<Task<bool>>();

        foreach (var workflowDefinition in workflowDefinitions)
        foreach (var workflowType in workflowDefinition.WorkflowTypes)
        {
            var writer = _writers.FirstOrDefault(w => w.WorkflowType == workflowType.GetType());

            if (writer is null)
                continue;

            var workflow = workflowResolver.Resolve(workflowDefinition);
            var checkTask = writer.CheckForDirtyWorkflow(workflow);

            checkTasks.Add(checkTask);
        }

        var result = await Task.WhenAll(checkTasks);

        return result.Any(x => x);
    }
}
