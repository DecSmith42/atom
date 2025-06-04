namespace DecSm.Atom.Reports;

/// <summary>
///     Defines a contract for reporting build execution outcomes and associated data.
///     Implementations provide different output formats and destinations for build results.
/// </summary>
/// <remarks>
///     This interface is used by the build executor to generate outcome reports after target execution.
///     Multiple implementations can be registered to provide simultaneous reporting to different destinations
///     (e.g., console output, CI/CD system summaries, file exports).
///     The reporting is performed asynchronously after all build targets have completed execution,
///     regardless of whether they succeeded or failed. Error handling is managed by the caller
///     to ensure that reporting failures don't affect the overall build process.
/// </remarks>
[PublicAPI]
public interface IOutcomeReportWriter
{
    /// <summary>
    ///     Generates and outputs a report of the build execution outcome.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A task that represents the asynchronous reporting operation.
    ///     The task completes when the report has been successfully generated and output.
    /// </returns>
    /// <remarks>
    ///     This method is called by the build executor after all targets have completed execution.
    ///     Implementations should:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Access build state and report data through injected services</description>
    ///         </item>
    ///         <item>
    ///             <description>Format the information according to their specific output requirements</description>
    ///         </item>
    ///         <item>
    ///             <description>Handle any output-specific errors gracefully</description>
    ///         </item>
    ///         <item>
    ///             <description>Complete quickly to avoid delaying build completion</description>
    ///         </item>
    ///     </list>
    ///     Common implementation patterns include:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Console output with formatted tables and summaries</description>
    ///         </item>
    ///         <item>
    ///             <description>File generation for CI/CD system consumption</description>
    ///         </item>
    ///         <item>
    ///             <description>Integration with external reporting systems</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    Task ReportRunOutcome(CancellationToken cancellationToken);
}
