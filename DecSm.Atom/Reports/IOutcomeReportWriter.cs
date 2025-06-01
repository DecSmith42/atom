namespace DecSm.Atom.Reports;

[PublicAPI]
public interface IOutcomeReportWriter
{
    Task ReportRunOutcome();
}
