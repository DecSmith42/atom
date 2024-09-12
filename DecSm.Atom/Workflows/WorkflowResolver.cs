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

        // Turn command steps into jobs
        var definedCommandJobs = definedSteps
            .OfType<CommandWorkflowStep>()
            .Select(step => new WorkflowJobModel(step.Name, [step])
            {
                Options = step.Options,
                MatrixDimensions = step.MatrixDimensions,
                JobDependencies = buildModel
                    .Targets
                    .Single(target => target.Name == step.Name)
                    .Dependencies
                    .Select(x => x.Name)
                    .ToList(),
            })
            .ToList();

        // If this workflow uses a custom artifact provider, we need to ensure that steps that
        // consume or produce artifacts are dependent on the Setup step.
        // It will be up to the WorkflowWriter to implement the download/upload steps.
        if (UseCustomArtifactProvider.IsEnabled(workflowOptions))
            definedCommandJobs = definedCommandJobs.ConvertAll(job => job
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

        // Check that all consumed variables are produced by the target they are consumed from to avoid errors later on
        foreach (var job in definedCommandJobs)
        {
            var target = buildModel.Targets.Single(x => x.Name == job.Name);

            foreach (var consumedVariable in target.ConsumedVariables)
            {
                var consumedTarget = buildModel.Targets.Single(x => x.Name == consumedVariable.TargetName);
                var jobOutput = consumedTarget.ProducedVariables.SingleOrDefault(x => x == consumedVariable.VariableName);

                if (jobOutput is null)
                    throw new InvalidOperationException(
                        $"Target '{job.Name}' consumes variable '{consumedVariable.VariableName}' from target '{consumedVariable.TargetName}', which does not produce that variable.");
            }
        }

        // Add all targets that are not already defined as jobs
        var commandJobs = definedCommandJobs
            .Concat(buildModel
                .Targets
                .Select(target => target.Name)
                .Where(targetName => definedCommandJobs.All(job => job.Name != targetName))
                .Select(targetName => new WorkflowJobModel(targetName, [new CommandWorkflowStep(targetName)])
                {
                    JobDependencies = [],
                    Options = [],
                    MatrixDimensions = [],
                }))
            .ToList();

        // Remove jobs that are not defined and not depended on by any other defined or dependent job
        while (commandJobs.Count > 0)
        {
            var removedJob = false;

            foreach (var job in commandJobs.Where(job => definedCommandJobs.All(definedJob => definedJob.Name != job.Name)))
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

        // Turn non-command steps into jobs and combine with ordered command jobs
        var allJobs = definedSteps
            .Where(step => step is not CommandWorkflowStep)
            .Select(step => new WorkflowJobModel(step.Name, [step])
            {
                JobDependencies = [],
                Options = [],
                MatrixDimensions = [],
            })
            .Concat(OrderJobs(commandJobs))
            .ToList();

        // Order jobs based on dependencies

        return new(definition.Name)
        {
            Triggers = definition.Triggers,
            Options = workflowOptions,
            Jobs = allJobs,
        };
    }

    private static List<WorkflowJobModel> OrderJobs(List<WorkflowJobModel> jobs)
    {
        var orderedJobs = new List<WorkflowJobModel>();

        foreach (var job in jobs)
            AddJob(job);

        return orderedJobs;

        void AddJob(WorkflowJobModel target)
        {
            foreach (var jobDep in target.JobDependencies)
                AddJob(jobs.Single(x => x.Name == jobDep));

            if (orderedJobs.Contains(target))
                return;

            orderedJobs.Add(target);
        }
    }
}
