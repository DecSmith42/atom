namespace DecSm.Atom.Module.GithubWorkflows;

internal sealed class GithubSummaryOutcomeReporter(IAtomFileSystem fileSystem, ReportService reportService) : IOutcomeReporter
{
    public async Task ReportRunOutcome()
    {
        await using var writer = fileSystem.File.OpenWrite(Github.Variables.StepSummary);

        await writer.WriteAsync(Encoding.UTF8.GetBytes(ReportDataMarkdownWriter.Write(reportService.GetReportData())));
    }
}
