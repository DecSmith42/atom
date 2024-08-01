namespace DecSm.Atom.Build;

internal interface IBuildExecutor
{
    Task Execute();
}

internal sealed class BuildExecutor(
    CommandLineArgs args,
    BuildModel buildModel,
    IParamService paramService,
    IWorkflowVariableService variableService,
    IEnumerable<IOutcomeReporter> outcomeReporters,
    IAnsiConsole console,
    IReportService reportService,
    ILogger<BuildExecutor> logger
) : IBuildExecutor
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

        foreach (var outcomeReporter in outcomeReporters)
            try
            {
                await outcomeReporter.ReportRunOutcome();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while reporting run outcome");
            }
    }

    private async Task ExecuteTarget(TargetModel target, StatusContext context)
    {
        if (buildModel.TargetStates[target].Status is TargetRunState.NotRun
            or TargetRunState.Skipped
            or TargetRunState.Succeeded
            or TargetRunState.Failed)
            return;

        if (buildModel.TargetStates[target].Status is not TargetRunState.PendingRun)
        {
            logger.LogWarning("Skipping target {TargetDefinitionName} due to unexpected state {TargetState}",
                target.Name,
                buildModel.TargetStates[target].Status);

            return;
        }

        foreach (var dependency in target.Dependencies)
            await ExecuteTarget(dependency, context);

        if (target.Dependencies.Any(depTarget => buildModel.TargetStates[depTarget].Status is TargetRunState.Failed))
        {
            buildModel.TargetStates[target].Status = TargetRunState.NotRun;
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
            if (args.HasHeadless)
            {
                console.WriteLine($"##[group]{target.Name}");
            }
            else
            {
                console.Write(new Markup($"[underline]{target.Name}[/]"));
                console.WriteLine();
                console.WriteLine();
            }

            if (target.Description is { Length: > 0 })
            {
                console.WriteLine($"Description: {target.Description}");
                console.WriteLine();
            }

            try
            {
                foreach (var task in target.Tasks)
                    await task();

                buildModel.TargetStates[target].Status = TargetRunState.Succeeded;
            }
            catch (StepFailedException failedCheckException)
            {
                logger.LogInformation(failedCheckException, "A check failed for target {TargetDefinitionName}", target.Name);
                buildModel.TargetStates[target].Status = TargetRunState.Failed;

                reportService.AddReportData(new TextReportData(failedCheckException.Message)
                {
                    Title = $"Check failed in {target.Name}",
                });

                if (failedCheckException.ReportData is not null)
                    reportService.AddReportData(failedCheckException.ReportData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing target {TargetDefinitionName}", target.Name);
                buildModel.TargetStates[target].Status = TargetRunState.Failed;
            }

            buildModel.TargetStates[target].RunDuration =
                TimeSpan.FromSeconds((Stopwatch.GetTimestamp() - startTime) / (double)Stopwatch.Frequency);

            if (args.HasHeadless)
                console.WriteLine("##[endgroup]");
        }
    }
}