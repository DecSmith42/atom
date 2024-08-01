namespace DecSm.Atom.Workflows;

internal static class WorkflowBuilder
{
    public static WorkflowModel BuildWorkflow(
        WorkflowDefinition definition,
        IBuildDefinition buildDefinition,
        BuildModel buildModel,
        IEnumerable<IWorkflowOptionProvider> workflowOptionProviders)
    {
        if (definition.StepDefinitions.Count == 0)
            return new(definition.Name)
            {
                Triggers = definition.Triggers,
                Options = definition.Options,
                Jobs = [],
            };

        var definedSteps = definition
            .StepDefinitions
            .Select(stepDefinition => stepDefinition.CreateStep(buildDefinition))
            .ToList();

        var definedNonTargetJobs = definedSteps
            .Where(step => step is not CommandWorkflowStep)
            .Select(step => new WorkflowJobModel(step.Name, [step]))
            .ToList();

        var definedTargetJobs = definedSteps
            .OfType<CommandWorkflowStep>()
            .Select(step => new WorkflowJobModel(step.Name, [step])
            {
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

            var job = commandJobMap[jobName];

            job.JobDependencies.AddRange(target.Dependencies.Select(x => x.Name));

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

        var options = definition
            .Options
            .Concat(buildDefinition.DefaultWorkflowOptions)
            .Concat(workflowOptionProviders.SelectMany(x => x.WorkflowOptions))
            .Distinct()
            .ToList();

        return new(definition.Name)
        {
            Triggers = definition.Triggers,
            Options = options,
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