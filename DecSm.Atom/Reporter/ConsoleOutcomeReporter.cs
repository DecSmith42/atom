namespace DecSm.Atom.Reporter;

public class ConsoleOutcomeReporter(CommandLineArgs args, IAnsiConsole console) : IOutcomeReporter
{
    public Task ReportRunOutcome(IReadOnlyList<TargetState> states)
    {
        var table = new Table()
            .Title("Build Summary")
            .Alignment(Justify.Left)
            .HideHeaders()
            .NoBorder()
            .AddColumn("Target")
            .AddColumn("Outcome")
            .AddColumn("Duration");

        foreach (var state in states.Where(x => x.Status is not TargetRunState.Skipped))
        {
            var outcome = state.Status switch
            {
                TargetRunState.Succeeded => "[green]Succeeded[/]",
                TargetRunState.Failed => "[red]Failed[/]",
                TargetRunState.NotRun => "[yellow]NotRun[/]",
                var runState => $"[red]Unexpected state: {runState}[/]",
            };

            var targetDuration = state.RunDuration;

            var durationText = state.Status is TargetRunState.Succeeded or TargetRunState.Failed
                ? $"{targetDuration?.TotalSeconds:0.00}s"
                : string.Empty;

            if (state.Status is TargetRunState.NotRun && args.HasHeadless)
                continue;

            table.AddRow(state.Name, outcome, durationText);
        }

        console.WriteLine();
        console.Write(table);
        console.WriteLine();

        return Task.CompletedTask;
    }
}