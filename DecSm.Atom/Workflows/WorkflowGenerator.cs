namespace DecSm.Atom.Workflows;

internal sealed class WorkflowGenerator(
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    IEnumerable<IWorkflowWriter> writers,
    IEnumerable<IWorkflowOptionProvider> workflowOptionProviders,
    ILogger<WorkflowGenerator> logger
) : IWorkflowGenerator
{
    private readonly List<IWorkflowWriter> _writers = writers.ToList();

    public void GenerateWorkflows()
    {
        logger.LogInformation("Generating workflows");

        var workflowDefinitions = ((BuildDefinition)buildDefinition).Workflows;

        foreach (var workflowDefinition in workflowDefinitions)
        foreach (var workflowType in workflowDefinition.WorkflowTypes)
            _writers
                .FirstOrDefault(w => w.WorkflowType == workflowType.GetType())
                ?.Generate(WorkflowBuilder.BuildWorkflow(workflowDefinition, buildDefinition, buildModel, workflowOptionProviders));

        logger.LogInformation("Workflows generated");
    }
}