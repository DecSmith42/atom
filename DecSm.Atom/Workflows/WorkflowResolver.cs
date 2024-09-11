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
        var workflowOptions = IWorkflowOption
            .MergeOptions(buildDefinition
                .DefaultWorkflowOptions
                .Concat(_workflowOptionProviders.SelectMany(provider => provider.WorkflowOptions))
                .Concat(definition.Options))
            .ToList();

        if (definition.StepDefinitions.Count is 0)
            return new(definition.Name)
            {
                Triggers = definition.Triggers,
                Options = workflowOptions,
                Jobs = [],
            };

        var definedSteps = definition
            .StepDefinitions
            .Select(stepDefinition => stepDefinition.CreateStep())
            .ToList();

        var definedNonTargetJobs = definedSteps
            .Where(step => step is not CommandWorkflowStep)
            .Select(step => new WorkflowJobModel(step.Name, [step]))
            .ToList();

        var definedTargetJobs = definedSteps
            .OfType<CommandWorkflowStep>()
            .Select(step => new WorkflowJobModel(step.Name, [step])
            {
                Options = step.Options,
                MatrixDimensions = step.MatrixDimensions,
            })
            .ToList();

        var commandJobMap = definedTargetJobs.ToDictionary(job => job.Name);

        foreach (var targetName in buildModel.Targets.Select(x => x.Name))
            if (!commandJobMap.ContainsKey(targetName))
                commandJobMap[targetName] = new(targetName, [new CommandWorkflowStep(targetName)]);

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
