namespace DecSm.Atom.Extensions.DevopsWorkflows;

internal sealed class DevopsSummaryOutcomeReporter(IAtomFileSystem fileSystem, IReportService reportService) : IOutcomeReporter
{
    public async Task ReportRunOutcome()
    {
        var tempFile = fileSystem.AtomTempDirectory / "summary.md";

        if (tempFile.FileExists)
            fileSystem.File.Delete(tempFile);

        try
        {
            await using (var writer = fileSystem.File.Create(tempFile))
                await writer.WriteAsync(Encoding.UTF8.GetBytes(ReportDataMarkdownWriter.Write(reportService.GetReportData())));

            // TODO: Debug code, remove
            var fileContent = await fileSystem.File.ReadAllTextAsync(tempFile);
            Console.WriteLine($"Contents of \"{tempFile}\":\n{fileContent}");

            Console.WriteLine($"##vso[task.uploadsummary]{tempFile}");
        }
        finally
        {
            if (tempFile.FileExists)
                fileSystem.File.Delete(tempFile);
        }
    }
}
