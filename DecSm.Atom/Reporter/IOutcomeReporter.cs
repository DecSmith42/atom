namespace DecSm.Atom.Reporter;

public interface IOutcomeReporter
{
    Task ReportRunOutcome(IReadOnlyList<TargetState> states);
}