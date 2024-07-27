namespace DecSm.Atom;

public sealed class CheckFailedException(string message, ICustomReportData? reportData = null) : Exception(message)
{
    public ICustomReportData? ReportData { get; } = reportData;
}