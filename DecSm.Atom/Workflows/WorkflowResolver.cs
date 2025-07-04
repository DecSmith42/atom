﻿namespace DecSm.Atom.Workflows;

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
        var workflowOptions = IWorkflowOption
            .Merge(buildDefinition
                .GlobalWorkflowOptions
                .Concat(_workflowOptionProviders.SelectMany(provider => provider.WorkflowOptions))
                .Concat(definition.Options))
            .ToList();

        // If there are no steps, we can return a simple workflow
        if (definition.Targets.Count is 0)
            return new(definition.Name)
            {
                Triggers = definition.Triggers,
                Options = workflowOptions,
                Jobs = [],
            };

        // Transform all step definitions into steps
        var definedSteps = definition
            .Targets
            .Select(targetDefinition => targetDefinition.CreateModel())
            .ToList();

        // Turn command steps into jobs
        var definedCommandJobs = definedSteps.ConvertAll(step => new WorkflowJobModel(step.Name, [step])
        {
            Options = step.Options,
            MatrixDimensions = step.MatrixDimensions,
            JobDependencies = buildModel
                .GetTarget(step.Name)
                .Dependencies
                .Select(x => x.Name)
                .ToList(),
        });

        // If this workflow uses a custom artifact provider, we need to ensure that steps that
        // consume or produce artifacts are dependent on the Setup step.
        // It will be up to the WorkflowWriter to implement the download/upload steps.
        if (UseCustomArtifactProvider.IsEnabled(workflowOptions))
            definedCommandJobs = definedCommandJobs.ConvertAll(job => job
                .Steps
                .Where(step => step is { SuppressArtifactPublishing: false })
                .Select(step => buildModel.GetTarget(step.Name))
                .Any(target => target.ConsumedArtifacts.Count > 0 || target.ProducedArtifacts.Count > 0)
                ? job with
                {
                    JobDependencies = job
                        .JobDependencies
                        .Append(nameof(ISetupBuildInfo.SetupBuildInfo))
                        .ToList(),
                }
                : job);

        // Check that all consumed variables are produced by the target they are consumed from to avoid errors later on
        foreach (var job in definedCommandJobs)
        {
            var target = buildModel.GetTarget(job.Name);

            foreach (var consumedVariable in target.ConsumedVariables)
            {
                var consumedTarget = buildModel.GetTarget(consumedVariable.TargetName);
                var jobOutput = consumedTarget.ProducedVariables.SingleOrDefault(x => x == consumedVariable.VariableName);

                if (jobOutput is null)
                    throw new InvalidOperationException(
                        $"Target '{job.Name}' consumes variable '{consumedVariable.VariableName}' from target '{consumedVariable.TargetName}', which does not produce that variable.");
            }
        }

        // Add all targets that are not already defined as jobs
        var jobs = definedCommandJobs
            .Concat(buildModel
                .Targets
                .Select(target => target.Name)
                .Where(targetName => definedCommandJobs.All(job => job.Name != targetName))
                .Select(targetName => new WorkflowJobModel(targetName, [new(targetName)])
                {
                    JobDependencies = [],
                    Options = [],
                    MatrixDimensions = [],
                }))
            .ToList();

        // Remove jobs that are not defined and not depended on by any other defined or dependent job
        while (jobs.Count > 0)
        {
            var removedJob = false;

            foreach (var job in jobs.Where(job => definedCommandJobs.All(definedJob => definedJob.Name != job.Name)))
            {
                if (jobs
                    .Where(x => x != job)
                    .SelectMany(x => x.JobDependencies)
                    .Any(x => x == job.Name))
                    continue;

                jobs.Remove(job);
                removedJob = true;

                break;
            }

            if (!removedJob)
                break;
        }

        // Order jobs based on dependencies

        return new(definition.Name)
        {
            Triggers = definition.Triggers,
            Options = workflowOptions,
            Jobs = OrderJobs(jobs),
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
