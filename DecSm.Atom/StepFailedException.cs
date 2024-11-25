namespace DecSm.Atom;

[PublicAPI]
public sealed class StepFailedException(string message, Exception? innerException = null) : Exception(message, innerException)
{
    public StepFailedException() : this(string.Empty) { }

    public StepFailedException(string? message) : this(message ?? string.Empty, null) { }

    public ICustomReportData? ReportData { get; init; }
}
