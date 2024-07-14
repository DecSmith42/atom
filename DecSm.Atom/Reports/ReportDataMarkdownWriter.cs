namespace DecSm.Atom.Reports;

public static class ReportDataMarkdownWriter
{
    public static string Write(IReadOnlyList<IReportData> reportData)
    {
        var builder = new StringBuilder();

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
            Write(builder, data);

        if (logReportData.Count != 0)
            Write(builder, logReportData);

        if (artifactReportData.Count != 0)
            Write(builder, artifactReportData);

        foreach (var data in customReportData)
            Write(builder, data);

        return builder.ToString();
    }

    public static void Write(StringBuilder builder, List<LogReportData> reportData)
    {
        var groups = reportData
            .GroupBy(x => x.Level)
            .OrderByDescending(x => x.Key)
            .ToList();

        Write(builder,
            groups
                .FirstOrDefault(x => x.Key is LogLevel.Critical)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Critical Errors",
            "CAUTION");

        Write(builder,
            groups
                .FirstOrDefault(x => x.Key is LogLevel.Error)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Errors",
            "CAUTION");

        Write(builder,
            groups
                .FirstOrDefault(x => x.Key is LogLevel.Warning)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Warnings",
            "WARNING");
    }

    public static void Write(StringBuilder builder, List<LogReportData> reportData, string header, string infoType)
    {
        if (reportData.Count == 0)
            return;

        builder.AppendLine($"## {header}");
        builder.AppendLine();

        foreach (var log in reportData)
        {
            builder.AppendLine($"> [!{infoType}]{log.Message}");
            builder.AppendLine("> ");
            builder.AppendLine($"> {log.Timestamp:h:mm:ss tt zz}");

            if (log.Exception is not null)
            {
                builder.AppendLine("> <details>");
                builder.AppendLine("> <summary>Exception</summary>");
                builder.AppendLine("> ");
                builder.AppendLine($"> `{log.Exception}`");
                builder.AppendLine("> ");
                builder.AppendLine("> </details>");
            }

            builder.AppendLine();
        }
    }

    public static void Write(StringBuilder builder, IEnumerable<ArtifactReportData> reportData)
    {
        builder.AppendLine("## Output Artifacts");
        builder.AppendLine();

        builder.AppendLine("| Name | Path |");
        builder.AppendLine("| ---- | ---- |");

        foreach (var artifactReportData in reportData)
            builder.AppendLine($"| {artifactReportData.Name} | [{artifactReportData.Path}]({artifactReportData.Path}) |");

        builder.AppendLine();
    }

    public static void Write(StringBuilder builder, ICustomReportData reportData)
    {
        switch (reportData)
        {
            case TableReportData tableReportData:
                Write(builder, tableReportData);

                break;

            case ListReportData listReportData:
                Write(builder, listReportData);

                break;

            case TextReportData textReportData:
                Write(builder, textReportData);

                break;

            default:
                builder.AppendLine(reportData.ToString() ?? string.Empty);
                builder.AppendLine();

                break;
        }
    }

    public static void Write(StringBuilder builder, TableReportData reportData)
    {
        if (reportData.Title is not null)
        {
            builder.AppendLine($"## {reportData.Title}");
            builder.AppendLine();
        }

        var columnCount = reportData
            .Rows
            .Append(reportData.Header ?? [])
            .Max(row => row.Count);

        var joinedRows = reportData
            .Rows
            .Prepend(Enumerable.Repeat("-", columnCount))
            .Prepend(reportData.Header ?? Enumerable.Repeat(string.Empty, columnCount))
            .Select(row =>
            {
                var rowArray = row.ToArray();

                return $"| {string.Join(" | ", rowArray)} |";
            })
            .ToList();

        foreach (var row in joinedRows)
            builder.AppendLine(row);

        builder.AppendLine();
    }

    public static void Write(StringBuilder builder, ListReportData reportData)
    {
        if (reportData.Title is not null)
        {
            builder.AppendLine($@"## {reportData.Title}");
            builder.AppendLine();
        }

        foreach (var item in reportData.Items)
            builder.AppendLine($"- {item}");

        builder.AppendLine();
    }

    public static void Write(StringBuilder builder, TextReportData reportData)
    {
        if (reportData.Title is not null)
        {
            builder.AppendLine($@"## {reportData.Title}");
            builder.AppendLine();
        }

        builder.AppendLine(reportData.Text);
        builder.AppendLine();
    }
}