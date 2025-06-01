namespace DecSm.Atom.Workflows;

internal sealed class WorkflowGenerator(
    IBuildDefinition buildDefinition,
    IEnumerable<IWorkflowWriter> writers,
    WorkflowResolver workflowResolver
)
{
    private readonly List<IWorkflowWriter> _writers = writers.ToList();

    public async Task GenerateWorkflows()
    {
        var workflowDefinitions = ((BuildDefinition)buildDefinition).Workflows;

        var generationTasks = new List<Task>();

        // ReSharper disable LoopCanBeConvertedToQuery
        foreach (var workflowDefinition in workflowDefinitions)
        foreach (var workflowType in workflowDefinition.WorkflowTypes)
        {
            // ReSharper restore LoopCanBeConvertedToQuery
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

        // ReSharper disable LoopCanBeConvertedToQuery
        foreach (var workflowDefinition in workflowDefinitions)
        foreach (var workflowType in workflowDefinition.WorkflowTypes)
        {
            // ReSharper restore LoopCanBeConvertedToQuery
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
