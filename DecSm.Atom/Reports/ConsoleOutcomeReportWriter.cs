namespace DecSm.Atom.Reports;

/// <summary>
///     Console-based implementation of <see cref="IOutcomeReportWriter" /> that provides formatted output for build
///     outcomes and report data.
/// </summary>
internal partial class ConsoleOutcomeReportWriter(
    CommandLineArgs args,
    IAnsiConsole console,
    BuildModel buildModel,
    ReportService reportService,
    IParamService paramService
) : IOutcomeReportWriter
{
    public Task ReportRunOutcome(CancellationToken cancellationToken)
    {
        var table = new Table()
            .LeftAligned()
            .HideHeaders()
            .Border(TableBorder.Minimal)
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
                TargetRunState.NotRun or TargetRunState.PendingRun => "[yellow]NotRun[/]",
                var runState => $"[red]Unexpected state: {runState}[/]",
            };

            var targetDuration = state.RunDuration;

            var durationText = state.Status is TargetRunState.Succeeded or TargetRunState.Failed &&
                               targetDuration is not null
                ? $"{targetDuration.Value.TotalSeconds:0.00}s"
                : string.Empty;

            if (state.Status is TargetRunState.NotRun && args.HasHeadless)
                continue;

            table.AddRow(paramService.MaskMatchingSecrets(state.Name), outcome, durationText);
        }

        console.Write(new Text("Build Summary", new(decoration: Decoration.Underline)));
        console.WriteLine();
        console.Write(table);
        console.WriteLine();

        if (!args.HasHeadless)
            Write(reportService.GetReportData());

        return Task.CompletedTask;
    }

    [return: NotNullIfNotNull("input")]
    private string? FormatFreetext(string? input) =>
        input is not null
            ? paramService.MaskMatchingSecrets(EmojiRegex()
                .Replace(input, string.Empty))
            : null;

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

        var tree = new Tree(new Text(header + "\n", new(decoration: Decoration.Underline)));

        foreach (var log in reportData)
        {
            var splitPattern = new[] { '\r', '\n' };
            var messageLines = log.Message.Split(splitPattern, StringSplitOptions.RemoveEmptyEntries);

            messageLines = messageLines
                .Select(paramService.MaskMatchingSecrets)
                .ToArray();

            var rows = new List<IRenderable>
            {
                new Markup($"[{styleTag}]{messageLines.FirstOrDefault().EscapeMarkup()}[/]"),
            };

            if (messageLines.Length > 1)
                rows.AddRange(messageLines
                    .Skip(1)
                    .Select(line => new Text(line)));

            if (log.Exception is not null)
                rows.Add(new Text(paramService.MaskMatchingSecrets(log.Exception.ToString())));

            if (rows.Count > 1 && log != reportData[^1])
                rows.Add(new Text(string.Empty));

            var node = new TreeNode(new Rows(rows));

            tree.AddNode(node);
        }

        console.Write(tree);
        console.WriteLine();
        console.WriteLine();
    }

    private void Write(List<ArtifactReportData> reportData)
    {
        if (reportData.Count == 0)
            return;

        var table = new Table()
            .Alignment(Justify.Left)
            .AddColumn("Name")
            .AddColumn("Path")
            .Border(TableBorder.Minimal);

        foreach (var artifact in reportData)
            table.AddRow(FormatFreetext(artifact.Name), FormatFreetext(artifact.Path));

        console.Write(new Text("Output Artifacts", new(decoration: Decoration.Underline)));
        console.WriteLine();
        console.Write(table);
        console.WriteLine();
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
                console.Write(new Text(FormatFreetext(reportData.ToString() ?? string.Empty)));
                console.WriteLine();

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

        var columnAlignments = reportData
            .ColumnAlignments
            .Concat(Enumerable.Repeat(ColumnAlignment.Left, columnCount - reportData.ColumnAlignments.Count))
            .ToArray();

        var headers = reportData
                          .Header
                          ?.Concat(Enumerable.Repeat(string.Empty, columnCount - reportData.Header.Count))
                          .ToArray() ??
                      Enumerable
                          .Repeat(string.Empty, columnCount)
                          .ToArray();

        headers = headers
            .Select(FormatFreetext)
            .ToArray()!;

        for (var i = 0; i < columnCount; i++)
        {
            var column = new TableColumn(headers[i])
            {
                Alignment = columnAlignments[i] switch
                {
                    ColumnAlignment.Left => Justify.Left,
                    ColumnAlignment.Center => Justify.Center,
                    ColumnAlignment.Right => Justify.Right,
                    _ => throw new UnreachableException(),
                },
            };

            table.AddColumn(column);
        }

        if (reportData.Header is null)
            table.HideHeaders();

        foreach (var row in reportData.Rows)
        {
            var formattedRow = row
                .Select(FormatFreetext)
                .ToArray();

            table.AddRow(formattedRow.Select(x => new Text(x!)));
        }

        if (reportData.Title is not null)
        {
            var title = FormatFreetext(reportData.Title);

            console.Write(new Text(title, new(decoration: Decoration.Underline)));
            console.WriteLine();
        }

        console.Write(table);
        console.WriteLine();
    }

    private void Write(ListReportData reportData)
    {
        var rows = reportData
            .Items
            .Select(x => new Text($"{FormatFreetext(reportData.Prefix)}{FormatFreetext(x)}"))
            .ToList();

        if (reportData.Title is not null)
        {
            var title = FormatFreetext(reportData.Title);

            console.Write(new Text(title, new(decoration: Decoration.Underline)));
            console.WriteLine();
            console.WriteLine();
        }

        console.Write(new Rows(rows));
        console.WriteLine();
        console.WriteLine();
    }

    private void Write(TextReportData reportData)
    {
        if (reportData.Title is not null)
        {
            var title = FormatFreetext(reportData.Title);
            console.Write(new Text(title, new(decoration: Decoration.Underline)));
            console.WriteLine();
            console.WriteLine();
        }

        var text = FormatFreetext(reportData.Text);
        console.WriteLine(text);
        console.WriteLine();
        console.WriteLine();
    }

    [GeneratedRegex(
        @"([\u2700-\u27BF]|[\uE000-\uF8FF]|\uD83C[\uDC00-\uDFFF]|\uD83D[\uDC00-\uDFFF]|[\u2011-\u26FF]|\uD83E[\uDD10-\uDDFF])")]
    private static partial Regex EmojiRegex();
}
