namespace DecSm.Atom;

public class AtomBuildExecutor(
    CommandLineArgs args,
    ExecutableBuild executableBuild,
    IParamService paramService,
    ILogger<AtomBuildExecutor> logger
)
{
    public async Task Execute()
    {
        logger.LogInformation("Executing build...");
        
        var commands = args.Commands;
        
        if (commands is { Length: 0 })
        {
            logger.LogInformation("No targets specified. Exiting...");
            
            return;
        }
        
        foreach (var command in commands)
            await ExecuteTarget(executableBuild.GetTarget(command.Name));
    }
    
    private async Task ExecuteTarget(ExecutableTarget target)
    {
        if (executableBuild.TargetStates[target] is not TargetRunState.PendingRun)
            return;
        
        foreach (var dependency in target.Dependencies)
            await ExecuteTarget(dependency);
        
        if (target.Dependencies.Any(depTarget => executableBuild.TargetStates[depTarget] is TargetRunState.Failed))
        {
            executableBuild.TargetStates[target] = TargetRunState.Skipped;
            logger.LogWarning("Skipping target {TargetDefinitionName} due to failed dependencies", target.TargetDefinition.Name);
            
            return;
        }
        
        foreach (var requirement in target.TargetDefinition.Requirements.Where(requirement =>
                     paramService.GetParam(requirement) is null or ""))
        {
            logger.LogError("Missing required parameter '{ParamName}' for target {TargetDefinitionName}",
                requirement,
                target.TargetDefinition.Name);
            
            executableBuild.TargetStates[target] = TargetRunState.Failed;
        }
        
        logger.LogInformation("Executing target {TargetDefinitionName}...", target.TargetDefinition.Name);
        
        executableBuild.TargetStates[target] = TargetRunState.Running;
        
        try
        {
            foreach (var task in target.TargetDefinition.Tasks)
                await task();
            
            executableBuild.TargetStates[target] = TargetRunState.Succeeded;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing target {TargetDefinitionName}", target.TargetDefinition.Name);
            executableBuild.TargetStates[target] = TargetRunState.Failed;
        }
    }
}