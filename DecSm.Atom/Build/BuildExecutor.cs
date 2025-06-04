namespace DecSm.Atom.Build;

internal sealed class BuildExecutor(
    CommandLineArgs args,
    BuildModel buildModel,
    IParamService paramService,
    IWorkflowVariableService variableService,
    IEnumerable<IOutcomeReportWriter> outcomeReporters,
    IAnsiConsole console,
    ReportService reportService,
    ILogger<BuildExecutor> logger
)
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        var commands = args.Commands;

        if (commands is { Count: 0 })
        {
            logger.LogInformation("No targets specified; execution skipped");

            return;
        }

        logger.LogInformation("Executing build");

        foreach (var command in commands)
            ValidateTargetParameters(buildModel.GetTarget(command.Name));

        foreach (var command in commands)
            await ExecuteTarget(buildModel.GetTarget(command.Name), cancellationToken);

        foreach (var outcomeReporter in outcomeReporters)
            try
            {
                await outcomeReporter.ReportRunOutcome(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while reporting run outcome");
            }

        if (buildModel.TargetStates.Values.Any(state => state.Status is TargetRunState.Failed or TargetRunState.NotRun))
            throw new StepFailedException("Build failed");
    }

    private void ValidateTargetParameters(TargetModel target)
    {
        foreach (var requiredParam in target.RequiredParams)
        {
            var defaultValue = requiredParam.DefaultValue is { Length: > 0 }
                ? requiredParam.DefaultValue
                : null;

            string? value;

            if (requiredParam.IsSecret)
            {
                value = paramService.GetParam(requiredParam.Name, defaultValue, x => x);
            }
            else
            {
                using var _ = paramService.CreateNoCacheScope();
                value = paramService.GetParam(requiredParam.Name, defaultValue);
            }

            if (value is { Length: > 0 })
                continue;

            logger.LogError("Missing required parameter '{ParamName}' for target {TargetDefinitionName}",
                requiredParam.ArgName,
                target.Name);

            buildModel.TargetStates[target].Status = TargetRunState.Failed;

            return;
        }
    }

    private async Task ExecuteTarget(TargetModel target, CancellationToken cancellationToken)
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
            await ExecuteTarget(dependency, cancellationToken);

        if (target.Dependencies.Any(depTarget => buildModel.TargetStates[depTarget].Status is TargetRunState.Failed))
        {
            buildModel.TargetStates[target].Status = TargetRunState.NotRun;
            logger.LogWarning("Skipping target {TargetDefinitionName} due to failed dependencies", target.Name);

            return;
        }

        foreach (var variable in target.ConsumedVariables)
        {
            await variableService.ReadVariable(variable.TargetName, variable.VariableName, cancellationToken);

            if (paramService.GetParam(variable.VariableName) is not (null or ""))
                continue;

            logger.LogError("Missing required variable '{VariableName}' from target {FromTarget} for target {Target}",
                variable,
                variable.TargetName,
                target.Name);

            buildModel.TargetStates[target].Status = TargetRunState.Failed;

            return;
        }

        buildModel.TargetStates[target].Status = TargetRunState.Running;

        var startTime = Stopwatch.GetTimestamp();

        console.WriteLine();
        console.Write(new Markup($"Executing target [bold]{target.Name}[/]...\n"));
        console.WriteLine();

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
                console.Write(new Markup($"[dim]{target.Description}[/]"));
                console.WriteLine();
                console.WriteLine();
            }

            try
            {
                foreach (var task in target.Tasks)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException("Build execution was cancelled", cancellationToken);

                    await task(cancellationToken);
                }

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
