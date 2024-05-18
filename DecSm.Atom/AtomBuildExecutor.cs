namespace DecSm.Atom;

public class AtomBuildExecutor(CommandLineArgs args, IAtomBuild build, ILogger<AtomBuildExecutor> logger)
{
    private HashSet<string> executedTargets = [];

    public async Task Execute()
    {
        logger.LogInformation("Executing build...");

        var commands = args.Commands;

        if (commands is { Length: 0 })
        {
            logger.LogInformation("No targets specified. Exiting...");
            return;
        }

        var plan = build.ExecutionPlan;

        foreach (var command in commands)
            await ExecuteTarget(plan.GetTarget(command.Name));
    }

    private List<ExecutableTarget> GetOrderedTargets()
    {
        var plan = build.ExecutionPlan;
        var allTargets = plan.Targets;
        var orderedTargets = new List<ExecutableTarget>(args.Commands.Select(x => plan.GetTarget(x.Name)));

        var orderRules = new Dictionary<ExecutableTarget, List<ExecutableTarget>>();

        foreach (var target in allTargets)
        {
            foreach (var dependency in target.Dependencies)
            {
                if (!orderRules.ContainsKey(dependency))
                    orderRules[dependency] = [];

                orderRules[dependency].Add(target);
            }

            foreach (var dependent in target.Dependents)
            {
                if (!orderRules.ContainsKey(target))
                    orderRules[target] = [];

                orderRules[target].Add(dependent);
            }
        }

        return orderedTargets;
    }

    private async Task ExecuteTarget(ExecutableTarget target)
    {
        if (executedTargets.Contains(target.TargetDefinition.Name))
            return;

        foreach (var dependency in target.Dependencies)
            await ExecuteTarget(target);

        logger.LogInformation("Executing target {TargetDefinitionName}...", target.TargetDefinition.Name);

        // Check requirements
        foreach (var requirement in target.TargetDefinition.Requirements)
        {
        }

        foreach (var task in target.TargetDefinition.Tasks)
            await task(build);

        executedTargets.Add(target.TargetDefinition.Name);
    }
}