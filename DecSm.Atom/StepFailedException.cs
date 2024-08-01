namespace DecSm.Atom;

public sealed class StepFailedException(string message, Exception? innerException = null) : Exception(message, innerException)
{
    public ICustomReportData? ReportData { get; init; }
}