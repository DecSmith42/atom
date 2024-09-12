using DecSm.Atom.Artifacts;

namespace DecSm.Atom.Workflows;

internal sealed class WorkflowResolver(
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    IEnumerable<IWorkflowOptionProvider> workflowOptionProviders
)
{
    private readonly IReadOnlyList<IWorkflowOptionProvider> _workflowOptionProviders = workflowOptionProviders.ToList();

    public WorkflowModel Resolve(WorkflowDefinition definition)
    {
        // Get all default options from BuildDefinition, WorkflowOptionProviders and WorkflowDefinition
        var workflowOptions = WorkflowOptionUtil
            .MergeOptions(buildDefinition
                .DefaultWorkflowOptions
                .Concat(_workflowOptionProviders.SelectMany(provider => provider.WorkflowOptions))
                .Concat(definition.Options))
            .ToList();

        // If there are no steps, we can return a simple workflow
        if (definition.StepDefinitions.Count is 0)
            return new(definition.Name)
            {
                Triggers = definition.Triggers,
                Options = workflowOptions,
                Jobs = [],
            };

        // Transform all step definitions into steps
        var definedSteps = definition
            .StepDefinitions
            .Select(stepDefinition => stepDefinition.CreateStep())
            .ToList();

        // Get the steps that are not Commands
        var definedNonTargetJobs = definedSteps
            .Where(step => step is not CommandWorkflowStep)
            .Select(step => new WorkflowJobModel(step.Name, [step]))
            .ToList();

        // Get the steps that are Commands
        var definedTargetJobs = definedSteps
            .OfType<CommandWorkflowStep>()
            .Select(step => new WorkflowJobModel(step.Name, [step])
            {
                Options = step.Options,
                MatrixDimensions = step.MatrixDimensions,
            })
            .ToList();

        // If this workflow uses a custom artifact provider, we need to ensure that steps that
        // consume or produce artifacts are dependent on the Setup step.
        // It will be up to the WorkflowWriter to implement the download/upload steps.
        if (UseCustomArtifactProvider.IsEnabled(workflowOptions))
            definedTargetJobs = definedTargetJobs.ConvertAll(job => job
                .Steps
                .Where(step => step is CommandWorkflowStep { SuppressArtifactPublishing: false })
                .Select(step => buildModel.Targets.Single(target => target.Name == step.Name))
                .Any(target => target.ConsumedArtifacts.Count > 0 || target.ProducedArtifacts.Count > 0)
                ? job with
                {
                    JobDependencies = job
                        .JobDependencies
                        .Append(nameof(ISetup.Setup))
                        .ToList(),
                }
                : job);

        // Map Command jobs by name for easy lookup
        var commandJobMap = definedTargetJobs.ToDictionary(job => job.Name);

        // Add all targets that are not already defined as jobs
        foreach (var targetName in buildModel.Targets.Select(x => x.Name))
            if (!commandJobMap.ContainsKey(targetName))
                commandJobMap[targetName] = new(targetName, [new CommandWorkflowStep(targetName)]);

        // Check that all consumed variables are produced by the target they are consumed from to avoid errors later on
        foreach (var jobName in commandJobMap.Keys)
        {
            var target = buildModel.Targets.Single(x => x.Name == jobName);

            foreach (var consumedVariable in target.ConsumedVariables)
            {
                var consumedTarget = buildModel.Targets.Single(x => x.Name == consumedVariable.TargetName);
                var jobOutput = consumedTarget.ProducedVariables.SingleOrDefault(x => x == consumedVariable.VariableName);

                if (jobOutput is null)
                    throw new InvalidOperationException(
                        $"Target '{jobName}' consumes variable '{consumedVariable.VariableName}' from target '{consumedVariable.TargetName}', which does not produce that variable.");
            }
        }

        var commandJobs = commandJobMap.Values.ToList();

        // Remove jobs that are not defined and not depended on by any other defined or dependent job
        while (commandJobs.Count > 0)
        {
            var removedJob = false;

            foreach (var job in commandJobs.Where(x => !definedTargetJobs.Contains(x)))
            {
                if (commandJobs
                    .Where(x => x != job)
                    .SelectMany(x => x.JobDependencies)
                    .Any(x => x == job.Name))
                    continue;

                commandJobs.Remove(job);
                removedJob = true;

                break;
            }

            if (!removedJob)
                break;
        }

        // Order jobs based on dependencies
        var jobs = definedNonTargetJobs
            .Concat(OrderJobs(commandJobs, commandJobMap))
            .ToList();

        return new(definition.Name)
        {
            Triggers = definition.Triggers,
            Options = workflowOptions,
            Jobs = jobs,
        };
    }

    private static List<WorkflowJobModel> OrderJobs(List<WorkflowJobModel> jobs, Dictionary<string, WorkflowJobModel> nameJobMap)
    {
        var orderedJobs = new List<WorkflowJobModel>();

        foreach (var job in jobs)
            AddJob(job);

        return orderedJobs;

        void AddJob(WorkflowJobModel target)
        {
            foreach (var jobDep in target.JobDependencies)
                AddJob(nameJobMap[jobDep]);

            if (orderedJobs.Contains(target))
                return;

            orderedJobs.Add(target);
        }
    }
}
