namespace DecSm.Atom.Build;

public class BuildExecutor(
    CommandLineArgs args,
    BuildModel buildModel,
    IParamService paramService,
    IWorkflowVariableService variableService,
    IAnsiConsole console,
    ILogger<BuildExecutor> logger
)
{
    public async Task Execute()
    {
        var commands = args.Commands;
        
        if (commands is { Length: 0 })
        {
            logger.LogInformation("No targets specified; execution skipped");
            
            return;
        }
        
        logger.LogInformation("Executing build");
        
        await console
            .Status()
            .StartAsync("Executing build...",
                async context =>
                {
                    foreach (var command in commands)
                        await ExecuteTarget(buildModel.GetTarget(command.Name), context);
                });
        
        var table = new Table()
            .Title("Build Summary")
            .Alignment(Justify.Left)
            .HideHeaders()
            .NoBorder()
            .AddColumn("Target")
            .AddColumn("Outcome")
            .AddColumn("Duration");
        
        foreach (var target in buildModel.Targets)
        {
            var targetState = buildModel.TargetStates[target];
            
            var outcome = targetState.Status switch
            {
                TargetRunState.Succeeded => "[green]Succeeded[/]",
                TargetRunState.Failed => "[red]Failed[/]",
                TargetRunState.Skipped => "[yellow]Skipped[/]",
                _ => null,
            };
            
            if (outcome is null)
                continue;
            
            var targetDuration = buildModel.TargetStates[target].RunDuration;
            
            var durationText = targetState.Status is TargetRunState.Succeeded or TargetRunState.Failed
                ? $"{targetDuration?.TotalSeconds:0.00}s"
                : "N/A";
            
            table.AddRow(target.Name, outcome, durationText);
        }
        
        console.WriteLine();
        console.Write(table);
        console.WriteLine();
    }
    
    private async Task ExecuteTarget(TargetModel target, StatusContext context)
    {
        if (buildModel.TargetStates[target].Status is not TargetRunState.PendingRun)
            return;
        
        foreach (var dependency in target.Dependencies)
            await ExecuteTarget(dependency, context);
        
        if (target.Dependencies.Any(depTarget => buildModel.TargetStates[depTarget].Status is TargetRunState.Failed))
        {
            buildModel.TargetStates[target].Status = TargetRunState.Skipped;
            logger.LogWarning("Skipping target {TargetDefinitionName} due to failed dependencies", target.Name);
            
            return;
        }
        
        foreach (var variable in target.ConsumedVariables)
        {
            await variableService.ReadVariable(variable.TargetName, variable.VariableName);
            
            if (paramService.GetParam(variable.VariableName) is not (null or ""))
                continue;
            
            logger.LogError("Missing required variable '{VariableName}' from target {FromTarget} for target {Target}",
                variable,
                variable.TargetName,
                target.Name);
            
            buildModel.TargetStates[target].Status = TargetRunState.Failed;
            
            return;
        }
        
        foreach (var requirement in target.RequiredParams.Where(requirement => paramService.GetParam(requirement) is null or ""))
        {
            logger.LogError("Missing required parameter '{ParamName}' for target {TargetDefinitionName}", requirement, target.Name);
            
            buildModel.TargetStates[target].Status = TargetRunState.Failed;
            
            return;
        }
        
        buildModel.TargetStates[target].Status = TargetRunState.Running;
        
        var startTime = Stopwatch.GetTimestamp();
        context.Status($"Executing [bold]{target.Name}[/]...");
        context.Spinner(Spinner.Known.Star);
        context.SpinnerStyle(Style.Parse("green"));
        
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["Command"] = target.Name,
               }))
        {
            try
            {
                if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null ||
                    Environment.GetEnvironmentVariable("AZURE_PIPELINES") is not null)
                {
                    console.WriteLine($"##[group]{target.Name}");
                }
                else
                {
                    console.Write(new Markup($"[underline]{target.Name}[/]"));
                    console.WriteLine();
                    console.WriteLine();
                }
                
                foreach (var task in target.Tasks)
                    await task();
                
                buildModel.TargetStates[target].Status = TargetRunState.Succeeded;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing target {TargetDefinitionName}", target.Name);
                buildModel.TargetStates[target].Status = TargetRunState.Failed;
            }
            finally
            {
                buildModel.TargetStates[target].RunDuration = new(Stopwatch.GetTimestamp() - startTime);
                
                if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null ||
                    Environment.GetEnvironmentVariable("AZURE_PIPELINES") is not null)
                    console.WriteLine("##[endgroup]");
            }
        }
    }
}