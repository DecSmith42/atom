namespace DecSm.Atom;

internal sealed class AtomService(
    CommandLineArgs args,
    IBuildExecutor executor,
    BuildModel buildModel,
    IHostApplicationLifetime lifetime,
    ILogger<AtomService> logger,
    ICheatsheetService cheatsheetService,
    IWorkflowGenerator workflowGenerator
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

            if (args.Args is { Count: 0 } or [VerboseArg])
            {
                await workflowGenerator.GenerateWorkflows();
                cheatsheetService.ShowCheatsheet();

                return;
            }

            if (args.HasHelp)
            {
                cheatsheetService.ShowCheatsheet();

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
