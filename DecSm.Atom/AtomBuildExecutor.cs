namespace DecSm.Atom;

public class AtomBuildExecutor(
    CommandLineArgs args,
    ExecutableBuild executableBuild,
    IParamService paramService,
    IAnsiConsole console,
    ILogger<AtomBuildExecutor> logger
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
                        await ExecuteTarget(executableBuild.GetTarget(command.Name), context);
                });
        
        var table = new Table()
            .Title("Build Summary")
            .Alignment(Justify.Left)
            .HideHeaders()
            .NoBorder()
            .AddColumn("Target")
            .AddColumn("Outcome")
            .AddColumn("Duration");
        
        // var stats = new BreakdownChart().Width(60);
        
        foreach (var target in executableBuild.Targets)
        {
            var targetState = executableBuild.TargetStates[target];
            
            var outcome = targetState switch
            {
                TargetRunState.Succeeded => "[green]Succeeded[/]",
                TargetRunState.Failed => "[red]Failed[/]",
                TargetRunState.Skipped => "[yellow]Skipped[/]",
                
                // _ => "[grey]Not run[/]",
                _ => null,
            };
            
            if (outcome is null)
                continue;
            
            var targetDuration = executableBuild.TargetDurations[target];
            
            // stats.AddItem(target.TargetDefinition.Name, targetDuration.TotalSeconds, Color);
            
            var durationText = targetState is TargetRunState.Succeeded or TargetRunState.Failed
                ? $"{targetDuration.TotalSeconds:0.00}s"
                : "N/A";
            
            table.AddRow(target.TargetDefinition.Name, outcome, durationText);
        }
        
        // var columns = new Columns(table, stats).Collapse();
        
        console.WriteLine();
        console.Write(table);
        console.WriteLine();
    }
    
    private async Task ExecuteTarget(ExecutableTarget target, StatusContext context)
    {
        if (executableBuild.TargetStates[target] is not TargetRunState.PendingRun)
            return;
        
        foreach (var dependency in target.Dependencies)
            await ExecuteTarget(dependency, context);
        
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
            
            return;
        }
        
        executableBuild.TargetStates[target] = TargetRunState.Running;
        
        var startTime = Stopwatch.GetTimestamp();
        context.Status($"Executing [bold]{target.TargetDefinition.Name}[/]...");
        context.Spinner(Spinner.Known.Star);
        context.SpinnerStyle(Style.Parse("green"));
        
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["Command"] = target.TargetDefinition.Name,
               }))
        {
            try
            {
                if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null ||
                    Environment.GetEnvironmentVariable("AZURE_PIPELINES") is not null)
                {
                    console.WriteLine($"##[group]{target.TargetDefinition.Name}");
                }
                else
                {
                    console.Write(new Markup($"[underline]{target.TargetDefinition.Name}[/]"));
                    console.WriteLine();
                    console.WriteLine();
                }
                
                foreach (var task in target.TargetDefinition.Tasks)
                    await task();
                
                executableBuild.TargetStates[target] = TargetRunState.Succeeded;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing target {TargetDefinitionName}", target.TargetDefinition.Name);
                executableBuild.TargetStates[target] = TargetRunState.Failed;
            }
            finally
            {
                executableBuild.TargetDurations[target] = new(Stopwatch.GetTimestamp() - startTime);
                
                if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null ||
                    Environment.GetEnvironmentVariable("AZURE_PIPELINES") is not null)
                    console.WriteLine("##[endgroup]");
            }
        }
    }
}