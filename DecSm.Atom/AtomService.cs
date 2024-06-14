namespace DecSm.Atom;

public class AtomService(
    CommandLineArgs args,
    BuildExecutor executor,
    BuildModel buildModel,
    IHostApplicationLifetime lifetime,
    ILogger<AtomService> logger,
    ICheatsheetService cheatsheetService,
    WorkflowGenerator workflowGenerator
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (args.Args is { Length: 0 })
            {
                cheatsheetService.ShowCheatsheet();

                return;
            }

            if (args.HasHelp)
                cheatsheetService.ShowCheatsheet();

            if (args.HasGen)
            {
                workflowGenerator.GenerateWorkflows();

                return;
            }

            await executor.Execute();

            if (buildModel.Targets.Any(x => buildModel.TargetStates[x].Status is TargetRunState.Failed))
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