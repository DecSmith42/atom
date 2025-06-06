namespace DecSm.Atom.Module.GithubWorkflows;

internal sealed class GithubSummaryOutcomeReportWriter(IAtomFileSystem fileSystem, ReportService reportService, IParamService paramService)
    : IOutcomeReportWriter
{
    public async Task ReportRunOutcome(CancellationToken cancellationToken)
    {
        await using var writer = fileSystem.File.OpenWrite(Github.Variables.StepSummary);

        var reportText = ReportDataMarkdownFormatter.Write(reportService.GetReportData());

        // If the text contains any secrets, we don't want to log it
        reportText = paramService.MaskMatchingSecrets(reportText);

        await writer.WriteAsync(Encoding.UTF8.GetBytes(reportText), cancellationToken);
    }
}
