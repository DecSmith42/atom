namespace DecSm.Atom;

public class AtomBuildService(
    CommandLineArgs args,
    AtomBuildExecutor executor,
    ExecutableBuild executableBuild,
    IHostApplicationLifetime lifetime,
    ILogger<AtomBuildService> logger,
    IAnsiConsole console,
    ICheatsheetService cheatsheetService,
    AtomWorkflowGenerator workflowGenerator
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        console.WriteLine();
        logger.LogInformation("Started");
        
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["$HideDate"] = true,
               }))
        {
            try
            {
                executableBuild.Init();
                
                if (args.Args is { Length: 0 })
                {
                    cheatsheetService.ShowCheatsheet();
                    
                    return;
                }
                
                if (args.HasHelp)
                    cheatsheetService.ShowCheatsheet();
                
                if (args.HasGen)
                {
                    logger.LogInformation("Generating workflows");
                    workflowGenerator.GenerateWorkflows();
                    logger.LogInformation("Workflows generated");
                    
                    return;
                }
                
                await executor.Execute();
                
                if (executableBuild.Targets.Any(x => executableBuild.TargetStates[x] is TargetRunState.Failed))
                    Environment.ExitCode = 1;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Stopped");
                Environment.ExitCode = 1;
            }
            finally
            {
                lifetime.StopApplication();
            }
        }
    }
}