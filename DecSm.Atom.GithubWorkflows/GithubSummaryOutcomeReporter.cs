namespace DecSm.Atom.GithubWorkflows;

internal sealed class GithubSummaryOutcomeReporter(IFileSystem fileSystem) : IOutcomeReporter
{
    public async Task ReportRunOutcome(IReadOnlyList<TargetState> states)
    {
        await using var writer = fileSystem.File.OpenWrite(Github.Variables.StepSummary);
        await writer.WriteAsync(Encoding.UTF8.GetBytes(GenerateSummaryMarkdown(states)));
    }

    private static string GenerateSummaryMarkdown(IReadOnlyList<TargetState> states)
    {
        var summaryBuilder = new StringBuilder();

        summaryBuilder.AppendLine("| 🏗️ Run | |");
        summaryBuilder.AppendLine("| - | - |");
        summaryBuilder.AppendLine($"| Pipeline | {Github.Variables.Workflow} |");
        summaryBuilder.AppendLine($"| Run ID | {Github.Variables.RunId} |");

        var atomVersion = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Single(x => x.GetName()
                             .Name ==
                         "DecSm.Atom")
            .GetName()
            .Version!;

        summaryBuilder.AppendLine($"| Atom Version | {atomVersion.Major}.{atomVersion.Minor}.{atomVersion.Build} |");

        summaryBuilder.AppendLine();

        foreach (var state in states.Where(x => x.Status is not TargetRunState.Skipped))
        {
            summaryBuilder.AppendLine();
            summaryBuilder.AppendLine($"| 🎯 {state.Name} | |");
            summaryBuilder.AppendLine("| - | - |");

            summaryBuilder.AppendLine($"| Status | {state.Status switch
            {
                TargetRunState.Succeeded => "✅ Succeeded",
                TargetRunState.Failed => "❌ Failed",
                TargetRunState.NotRun => "⛔ NotRun",
                _ => "❓ Unknown",
            }} |");

            if (state.RunDuration is { Ticks: > 0 })
                summaryBuilder.AppendLine($"| Duration | ⌚ {state.RunDuration.Value.TotalSeconds:F2}s |");

            foreach (var data in state.Data)
                summaryBuilder.AppendLine($"| {data.Key} | {data.Value} |");

            summaryBuilder.AppendLine();
        }

        summaryBuilder.AppendLine();

        return summaryBuilder.ToString();
    }
}