namespace DecSm.Atom;

internal sealed class AtomService(
    CommandLineArgs args,
    BuildExecutor executor,
    BuildModel buildModel,
    IHostApplicationLifetime lifetime,
    ILogger<AtomService> logger,
    IHelpService helpService,
    WorkflowGenerator workflowGenerator
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (!args.IsValid)
            {
                Environment.ExitCode = 1;

                return;
            }

            if (args is { HasHelp: false, HasHeadless: false, HasGen: false, Commands.Count: 0, Params.Count: 0 })
            {
                await workflowGenerator.GenerateWorkflows();
                helpService.ShowHelp();

                return;
            }

            if (args.HasHelp)
            {
                helpService.ShowHelp();

                return;
            }

            if (args.HasGen || !args.HasHeadless)
                await workflowGenerator.GenerateWorkflows();
            else if (await workflowGenerator.WorkflowsDirty())
                throw new InvalidOperationException("One or more workflows are dirty. Run 'atom -g' to regenerate them");

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
