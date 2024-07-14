namespace DecSm.Atom.GithubWorkflows;

internal sealed class GithubSummaryOutcomeReporter(IFileSystem fileSystem, IReportService reportService) : IOutcomeReporter
{
    public async Task ReportRunOutcome(IReadOnlyList<TargetState> states)
    {
        await using var writer = fileSystem.File.OpenWrite(Github.Variables.StepSummary);

        await writer.WriteAsync(Encoding.UTF8.GetBytes(ReportDataMarkdownWriter.Write(reportService.GetReportData())));
    }
}