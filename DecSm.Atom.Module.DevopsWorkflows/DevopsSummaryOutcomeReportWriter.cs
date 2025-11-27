namespace DecSm.Atom.Module.DevopsWorkflows;

internal sealed class DevopsSummaryOutcomeReportWriter(
    IAtomFileSystem fileSystem,
    ReportService reportService,
    IParamService paramService
) : IOutcomeReportWriter
{
    public async Task ReportRunOutcome(CancellationToken cancellationToken)
    {
        var tempFile = fileSystem.AtomTempDirectory / "DevopsSummaryOutcome.md";

        if (tempFile.FileExists)
            fileSystem.File.Delete(tempFile);

        var reportText = ReportDataMarkdownFormatter.Write(reportService.GetReportData());

        // If the text contains any secrets, we don't want to log it
        reportText = paramService.MaskMatchingSecrets(reportText);

        var content = Encoding.UTF8.GetBytes(reportText);

        if (content.Length is 0)
            return;

        await using (var writer = fileSystem.File.Create(tempFile))
            await writer.WriteAsync(content, cancellationToken);

        Console.WriteLine($"##vso[task.uploadsummary]{tempFile}");
    }
}
