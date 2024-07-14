namespace DecSm.Atom.Reports;

public interface IOutcomeReporter
{
    Task ReportRunOutcome(IReadOnlyList<TargetState> states);
}