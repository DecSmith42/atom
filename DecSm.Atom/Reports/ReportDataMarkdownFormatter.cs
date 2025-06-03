namespace DecSm.Atom.Reports;

/// <summary>
///     Provides functionality to convert structured report data into GitHub Flavored Markdown format.
/// </summary>
/// <remarks>
///     This formatter takes a collection of <see cref="IReportData" /> objects and produces a cohesive
///     Markdown document with proper formatting, sections, and styling. It handles multiple data types
///     including logs, artifacts, tables, lists, and custom text content.
///     <para>
///         The formatter organizes content in a specific order:
///         <list type="number">
///             <item>Custom report data marked with <see cref="ICustomReportData.BeforeStandardData" /> = true</item>
///             <item>Log data grouped by severity level (Critical, Error, Warning)</item>
///             <item>Artifact data as linked lists</item>
///             <item>Custom report data marked with <see cref="ICustomReportData.BeforeStandardData" /> = false</item>
///         </list>
///     </para>
///     <para>
///         The output uses GitHub Flavored Markdown features including callout syntax for logs,
///         properly formatted tables, and collapsible exception details.
///     </para>
/// </remarks>
[PublicAPI]
public static class ReportDataMarkdownFormatter
{
    /// <summary>
    ///     Converts a collection of report data into a formatted Markdown string.
    /// </summary>
    /// <param name="reportData">A read-only list of <see cref="IReportData" /> objects to be formatted.</param>
    /// <returns>A string containing the complete Markdown-formatted report.</returns>
    /// <remarks>
    ///     The method processes different data types in a specific order to ensure logical presentation:
    ///     <list type="bullet">
    ///         <item>Pre-positioned custom data appears first</item>
    ///         <item>Log data is grouped by severity and sorted chronologically within each group</item>
    ///         <item>Artifact data is presented as an "Output Artifacts" section with links</item>
    ///         <item>Post-positioned custom data appears last</item>
    ///     </list>
    ///     Empty sections are automatically skipped to avoid unnecessary whitespace in the output.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var reportData = new List&lt;IReportData&gt;
    /// {
    ///     new LogReportData("Build failed", null, LogLevel.Error, DateTimeOffset.Now),
    ///     new ArtifactReportData("📦 Build Output", "/path/to/output.zip"),
    ///     new TableReportData(new[] { new[] { "Tests", "5" }, new[] { "Passed", "3" } })
    ///     {
    ///         Title = "Test Summary",
    ///         Header = new[] { "Metric", "Value" }
    ///     }
    /// };
    /// string markdown = ReportDataMarkdownFormatter.Write(reportData);
    /// // Produces formatted Markdown with error callouts, artifact links, and tables
    /// </code>
    /// </example>
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

        Write(builder, logReportData);

        Write(builder, artifactReportData);

        foreach (var data in customReportData)
            Write(builder, data);

        return builder.ToString();
    }

    private static void Write(StringBuilder builder, List<LogReportData> reportData)
    {
        if (reportData.Count == 0)
            return;

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

    private static void Write(StringBuilder builder, List<LogReportData> reportData, string header, string infoType)
    {
        if (reportData.Count == 0)
            return;

        builder.AppendLine($"### {header}");
        builder.AppendLine();

        foreach (var log in reportData)
        {
            builder.AppendLine($"> [!{infoType}]");
            builder.AppendLine($"> {log.Message}");

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

        builder.AppendLine();
    }

    private static void Write(StringBuilder builder, List<ArtifactReportData> reportData)
    {
        if (reportData.Count == 0)
            return;

        builder.AppendLine("### Output Artifacts");
        builder.AppendLine();

        foreach (var artifactReportData in reportData)
            builder.AppendLine($"- [{artifactReportData.Name}]({artifactReportData.Path})");

        builder.AppendLine();
        builder.AppendLine();
    }

    private static void Write(StringBuilder builder, ICustomReportData reportData)
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

    private static void Write(StringBuilder builder, TableReportData reportData)
    {
        builder.AppendLine($"### {reportData.Title}");
        builder.AppendLine();

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
        builder.AppendLine();
    }

    private static void Write(StringBuilder builder, ListReportData reportData)
    {
        builder.AppendLine($"### {reportData.Title}");
        builder.AppendLine();

        foreach (var item in reportData.Items)
            builder.AppendLine($"{reportData.Prefix}{item}");

        builder.AppendLine();
        builder.AppendLine();
    }

    private static void Write(StringBuilder builder, TextReportData reportData)
    {
        builder.AppendLine($"### {reportData.Title}");
        builder.AppendLine();

        builder.AppendLine(reportData.Text);
        builder.AppendLine();
        builder.AppendLine();
    }
}
