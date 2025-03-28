﻿namespace DecSm.Atom.Module.DevopsWorkflows;

internal sealed class DevopsSummaryOutcomeReporter(IAtomFileSystem fileSystem, ReportService reportService) : IOutcomeReporter
{
    public async Task ReportRunOutcome()
    {
        var tempFile = fileSystem.AtomTempDirectory / "DevopsSummaryOutcome.md";

        if (tempFile.FileExists)
            fileSystem.File.Delete(tempFile);

        var content = Encoding.UTF8.GetBytes(ReportDataMarkdownWriter.Write(reportService.GetReportData()));

        if (content.Length is 0)
            return;

        await using (var writer = fileSystem.File.Create(tempFile))
            await writer.WriteAsync(content);

        Console.WriteLine($"##vso[task.uploadsummary]{tempFile}");
    }
}
