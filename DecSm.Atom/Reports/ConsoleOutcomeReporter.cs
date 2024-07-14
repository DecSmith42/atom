using Spectre.Console.Rendering;

namespace DecSm.Atom.Reports;

public class ConsoleOutcomeReporter(CommandLineArgs args, IAnsiConsole console, BuildModel buildModel, IReportService reportService)
    : IOutcomeReporter
{
    public Task ReportRunOutcome()
    {
        var table = new Table()
            .LeftAligned()
            .Title("Build Summary", new(decoration: Decoration.Bold))
            .HideHeaders()
            .AddColumn("Target")
            .AddColumn("Outcome")
            .AddColumn("Duration");

        foreach (var state in buildModel
                     .TargetStates
                     .Select(x => x.Value)
                     .Where(x => x.Status is not TargetRunState.Skipped))
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

        console.Write(table);

        if (!args.HasHeadless)
            Write(reportService.GetReportData());

        return Task.CompletedTask;
    }

    private void Write(IReadOnlyList<IReportData> reportData)
    {
        var customPreReportData = reportData
            .OfType<ICustomReportData>()
            .Where(x => x.BeforeStandardData)
            .ToList();

        var logReportData = reportData
            .OfType<LogReportData>()
            .ToList();

        var artifactReportData = reportData
            .OfType<ArtifactReportData>()
            .ToList();

        var customReportData = reportData
            .OfType<ICustomReportData>()
            .Where(x => !x.BeforeStandardData)
            .ToList();

        foreach (var data in customPreReportData)
            Write(data);

        Write(logReportData);

        Write(artifactReportData);

        foreach (var data in customReportData)
            Write(data);
    }

    private void Write(List<LogReportData> reportData)
    {
        if (reportData.Count == 0)
            return;

        var groups = reportData
            .GroupBy(x => x.Level)
            .OrderByDescending(x => x.Key)
            .ToList();

        Write(groups
                .FirstOrDefault(x => x.Key is LogLevel.Critical)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Critical Errors",
            "maroon");

        Write(groups
                .FirstOrDefault(x => x.Key is LogLevel.Error)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Errors",
            "maroon");

        Write(groups
                .FirstOrDefault(x => x.Key is LogLevel.Warning)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Warnings",
            "darkorange");
    }

    private void Write(List<LogReportData> reportData, string header, string styleTag)
    {
        if (reportData.Count == 0)
            return;

        var tree = new Tree(string.Empty);

        foreach (var log in reportData)
        {
            var messageLines = log.Message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var rows = new List<IRenderable>
            {
                new Markup($"[{styleTag}]{messageLines[0]}[/]"),
            };

            if (messageLines.Length > 1)
                rows.AddRange(messageLines
                    .Skip(1)
                    .Select(line => new Text(line)));

            if (log.Exception is not null)
                rows.Add(new Text(log.Exception.ToString()));

            var node = new TreeNode(new Rows(rows));

            tree.AddNode(node);
        }

        console.Write(new Panel(tree)
            .Header(header)
            .Padding(new(2, 1)));
    }

    private void Write(List<ArtifactReportData> reportData)
    {
        if (reportData.Count == 0)
            return;

        var table = new Table()
            .Title(new("Artifacts", new(decoration: Decoration.Bold)))
            .Alignment(Justify.Left)
            .AddColumn("Name")
            .AddColumn("Path");

        foreach (var artifact in reportData)
            table.AddRow(artifact.Name, artifact.Path);
    }

    private void Write(ICustomReportData reportData)
    {
        switch (reportData)
        {
            case TableReportData tableReportData:
                Write(tableReportData);

                break;

            case ListReportData listReportData:
                Write(listReportData);

                break;

            case TextReportData textReportData:
                Write(textReportData);

                break;

            default:
                console.Write(new Text(reportData.ToString() ?? string.Empty));

                break;
        }
    }

    private void Write(TableReportData reportData)
    {
        var table = new Table()
            .Alignment(Justify.Left)
            .Border(TableBorder.Minimal);

        var columnCount = reportData
            .Rows
            .Append(reportData.Header ?? [])
            .Max(row => row.Count);

        if (reportData.Header is not null)
            foreach (var headerColumn in reportData.Header.Concat(Enumerable.Repeat(string.Empty, columnCount - reportData.Header.Count)))
                table.AddColumn(headerColumn);
        else
            table
                .AddColumns(Enumerable
                    .Repeat(new TableColumn(string.Empty), columnCount)
                    .ToArray())
                .HideHeaders();

        foreach (var row in reportData.Rows)
            table.AddRow(row.Select(x => new Text(x)));

        var panel = new Panel(table);

        if (reportData.Title is not null)
            panel.Header(reportData.Title);

        console.Write(panel);
    }

    private void Write(ListReportData reportData)
    {
        var rows = reportData
            .Items
            .Select(x => new Text(x))
            .ToList();

        var panel = new Panel(new Rows(rows));

        if (reportData.Title is not null)
            panel.Header(reportData.Title, Justify.Left);

        console.Write(panel.Padding(new(2, 1)));
    }

    private void Write(TextReportData reportData)
    {
        var panel = new Panel(reportData.Text);

        if (reportData.Title is not null)
            panel.Header = new(reportData.Title);

        console.Write(panel.Padding(new(2, 1)));
    }
}