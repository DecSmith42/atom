namespace DecSm.Atom;

/// <summary>
///     Represents an exception that is thrown when a step in an automated process, test, or workflow fails.
///     This exception provides additional reporting capabilities through custom report data.
/// </summary>
/// <remarks>
///     This exception is typically used in step-based automation frameworks, testing environments,
///     or workflow engines where individual steps can fail and need to be reported with additional context.
///     The <see cref="ReportData" /> property allows attaching custom reporting information that can be
///     used for enhanced error reporting, logging, or debugging purposes.
/// </remarks>
[PublicAPI]
public sealed class StepFailedException(string message, Exception? innerException = null) : Exception(message, innerException)
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StepFailedException" /> class with an empty message.
    /// </summary>
    public StepFailedException() : this(string.Empty) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StepFailedException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error. If null, an empty string will be used.</param>
    public StepFailedException(string? message) : this(message ?? string.Empty, null) { }

    /// <summary>
    ///     Gets custom report data associated with this step failure.
    ///     This property can be used to attach additional context, diagnostic information,
    ///     or custom reporting data that helps with error analysis and reporting.
    /// </summary>
    /// <value>
    ///     An <see cref="ICustomReportData" /> instance containing custom report information,
    ///     or <c>null</c> if no custom report data is associated with this exception.
    /// </value>
    /// <remarks>
    ///     This property can only be set during object initialization (init-only property).
    ///     It's designed to provide extensible reporting capabilities for different types of step failures.
    /// </remarks>
    public ICustomReportData? ReportData { get; init; }
}
