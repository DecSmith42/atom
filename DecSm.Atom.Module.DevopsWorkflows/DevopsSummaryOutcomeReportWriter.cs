namespace DecSm.Atom.Module.DevopsWorkflows;

internal sealed class DevopsSummaryOutcomeReportWriter(IAtomFileSystem fileSystem, ReportService reportService) : IOutcomeReportWriter
{
    public async Task ReportRunOutcome(CancellationToken cancellationToken)
    {
        var tempFile = fileSystem.AtomTempDirectory / "DevopsSummaryOutcome.md";

        if (tempFile.FileExists)
            fileSystem.File.Delete(tempFile);

        var content = Encoding.UTF8.GetBytes(ReportDataMarkdownFormatter.Write(reportService.GetReportData()));

        if (content.Length is 0)
            return;

        await using (var writer = fileSystem.File.Create(tempFile))
            await writer.WriteAsync(content, cancellationToken);

        Console.WriteLine($"##vso[task.uploadsummary]{tempFile}");
    }
}
