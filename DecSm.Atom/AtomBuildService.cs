namespace DecSm.Atom;

public class AtomBuildService(
    CommandLineArgs args,
    AtomBuildExecutor executor,
    IHostApplicationLifetime lifetime,
    ILogger<AtomBuildService> logger,
    ICheatsheetService cheatsheetService,
    AtomWorkflowGenerator workflowGenerator
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Atom Build");

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
                logger.LogInformation("Generating workflows");
                workflowGenerator.GenerateWorkflows();
                logger.LogInformation("Workflows generated");
                return;
            }

            await executor.Execute();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing the build");
            Environment.ExitCode = 1;
        }
        finally
        {
            logger.LogInformation("Atom Build stopped");
            lifetime.StopApplication();
        }
    }
}