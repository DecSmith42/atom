namespace DecSm.Atom.Module.GithubWorkflows;

internal sealed class GithubSummaryOutcomeReportWriter(IAtomFileSystem fileSystem, ReportService reportService) : IOutcomeReportWriter
{
    public async Task ReportRunOutcome(CancellationToken cancellationToken)
    {
        await using var writer = fileSystem.File.OpenWrite(Github.Variables.StepSummary);

        await writer.WriteAsync(Encoding.UTF8.GetBytes(ReportDataMarkdownFormatter.Write(reportService.GetReportData())),
            cancellationToken);
    }
}
