namespace DecSm.Atom.Reports;

/// <summary>
///     Base interface for all report data types in the build reporting system.
///     Represents a unit of reportable information that can be collected during build execution
///     and rendered by various report writers.
/// </summary>
/// <remarks>
///     This interface serves as a marker for all types that can be included in build reports.
///     Implementations typically fall into two categories:
///     <list type="bullet">
///         <item>
///             <description>Standard data types (logs, artifacts) - automatically collected by the system</description>
///         </item>
///         <item>
///             <description>Custom data types - explicitly created by build targets for specific reporting needs</description>
///         </item>
///     </list>
/// </remarks>
[PublicAPI]
public interface IReportData;

/// <summary>
///     Interface for custom report data that provides additional control over report rendering.
///     Extends <see cref="IReportData" /> with formatting and positioning capabilities.
/// </summary>
/// <remarks>
///     Custom report data allows build targets to create rich, formatted output that integrates
///     seamlessly with standard system-generated reports. The positioning control enables
///     strategic placement of custom content relative to standard data types.
/// </remarks>
[PublicAPI]
public interface ICustomReportData : IReportData
{
    /// <summary>
    ///     Gets a value indicating whether this report data should be rendered before standard data types.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this data should appear before standard log and artifact data;
    ///     <c>false</c> if it should appear after standard data.
    /// </value>
    /// <remarks>
    ///     Standard data includes <see cref="LogReportData" /> and <see cref="ArtifactReportData" />.
    ///     This property allows custom reports to be positioned strategically in the output,
    ///     such as showing summary information before detailed logs, or conclusions after all other data.
    /// </remarks>
    bool BeforeStandardData { get; }
}

/// <summary>
///     Represents log message data captured during build execution for inclusion in reports.
///     Automatically collected by the reporting system for warning, error, and critical level messages.
/// </summary>
/// <param name="Message">The log message text, with any secrets masked for security.</param>
/// <param name="Exception">The associated exception, if any, that was logged with the message.</param>
/// <param name="Level">The severity level of the log entry (Warning, Error, Critical).</param>
/// <param name="Timestamp">The exact time when the log entry was created.</param>
/// <remarks>
///     Log report data is automatically collected by <see cref="ReportLogger" /> for messages
///     at Warning level and above. Debug, Trace, and Information level messages are excluded
///     to keep reports focused on actionable information.
///     The system automatically masks any secrets found in log messages to prevent
///     sensitive information from appearing in reports.
///     Report writers typically group log data by severity level and present it in
///     chronological order within each group.
/// </remarks>
[PublicAPI]
public sealed record LogReportData(string Message, Exception? Exception, LogLevel Level, DateTimeOffset Timestamp)
    : IReportData;

/// <summary>
///     Represents information about build artifacts (output files) generated during execution.
///     Used to track and report on files created by the build process.
/// </summary>
/// <param name="Name">The display name of the artifact, often including emojis or descriptive text.</param>
/// <param name="Path">The file system path where the artifact can be found.</param>
/// <remarks>
///     Artifact data is typically created by build targets that generate output files
///     such as compiled binaries, test results, coverage reports, or documentation.
///     The name field often includes emojis or formatting to make artifacts easily
///     identifiable in reports, while the path provides the exact location for
///     access or further processing.
///     Report writers commonly present artifacts in tabular format with both
///     name and path information for easy reference.
/// </remarks>
[PublicAPI]
public sealed record ArtifactReportData(string Name, string Path) : IReportData;

/// <summary>
///     Represents structured tabular data for inclusion in build reports.
///     Provides rich formatting options including headers, alignment, and titles.
/// </summary>
/// <param name="Rows">The data rows, where each row is a collection of cell values.</param>
/// <remarks>
///     Table report data is ideal for presenting structured information such as:
///     <list type="bullet">
///         <item>
///             <description>Test execution summaries with counts and durations</description>
///         </item>
///         <item>
///             <description>Code coverage statistics</description>
///         </item>
///         <item>
///             <description>Performance metrics and benchmarks</description>
///         </item>
///         <item>
///             <description>Configuration or parameter summaries</description>
///         </item>
///     </list>
///     The table automatically handles variable column counts and provides sensible
///     defaults for missing headers or alignment specifications.
/// </remarks>
[PublicAPI]
public sealed record TableReportData(IReadOnlyList<IReadOnlyList<string>> Rows) : ICustomReportData
{
    /// <summary>
    ///     Gets or initializes an optional title to display above the table.
    /// </summary>
    /// <value>The table title, or <c>null</c> if no title should be displayed.</value>
    public string? Title { get; init; }

    /// <summary>
    ///     Gets or initializes the column headers for the table.
    /// </summary>
    /// <value>
    ///     A list of header strings, or <c>null</c> to hide headers entirely.
    ///     If provided, the list will be padded with empty strings if it's shorter than the maximum row length.
    /// </value>
    public IReadOnlyList<string>? Header { get; init; }

    /// <summary>
    ///     Gets or initializes the alignment settings for each column.
    /// </summary>
    /// <value>
    ///     A list of <see cref="ColumnAlignment" /> values.
    ///     Defaults to an empty list, which results in left alignment for all columns.
    ///     If shorter than the column count, remaining columns default to left alignment.
    /// </value>
    public IReadOnlyList<ColumnAlignment> ColumnAlignments { get; init; } = [];

    /// <summary>
    ///     Gets or initializes a value indicating whether this table should appear before standard report data.
    /// </summary>
    /// <value><c>true</c> to position before logs and artifacts; <c>false</c> to position after.</value>
    public bool BeforeStandardData { get; init; }
}

/// <summary>
///     Represents an unordered list of items for inclusion in build reports.
///     Provides formatting options for prefixes and titles.
/// </summary>
/// <param name="Items">The list items to display.</param>
/// <remarks>
///     List report data is useful for presenting:
///     <list type="bullet">
///         <item>
///             <description>Collections of related information or findings</description>
///         </item>
///         <item>
///             <description>Hierarchical data with custom indentation</description>
///         </item>
///         <item>
///             <description>Failed test cases with detailed information</description>
///         </item>
///         <item>
///             <description>Validation results or recommendations</description>
///         </item>
///     </list>
///     The customizable prefix allows for different list styles (bullets, dashes, numbering)
///     or hierarchical indentation patterns.
/// </remarks>
[PublicAPI]
public sealed record ListReportData(IReadOnlyList<string> Items) : ICustomReportData
{
    /// <summary>
    ///     Gets or initializes an optional title to display above the list.
    /// </summary>
    /// <value>The list title, or <c>null</c> if no title should be displayed.</value>
    public string? Title { get; init; }

    /// <summary>
    ///     Gets or initializes the prefix to prepend to each list item.
    /// </summary>
    /// <value>
    ///     The prefix string. Defaults to "- " for standard bullet-point formatting.
    ///     Can be customized for different list styles or hierarchical indentation.
    /// </value>
    public string Prefix { get; init; } = "- ";

    /// <summary>
    ///     Gets or initializes a value indicating whether this list should appear before standard report data.
    /// </summary>
    /// <value><c>true</c> to position before logs and artifacts; <c>false</c> to position after.</value>
    public bool BeforeStandardData { get; init; }
}

/// <summary>
///     Represents free-form text content for inclusion in build reports.
///     Provides the most flexible option for custom report content.
/// </summary>
/// <param name="Text">The text content to display.</param>
/// <remarks>
///     Text report data is suitable for:
///     <list type="bullet">
///         <item>
///             <description>Detailed explanations or summaries</description>
///         </item>
///         <item>
///             <description>Multi-line formatted content</description>
///         </item>
///         <item>
///             <description>Error messages or failure descriptions</description>
///         </item>
///         <item>
///             <description>Custom formatted output that doesn't fit other data types</description>
///         </item>
///     </list>
///     The text can contain multiple lines and will be rendered preserving
///     the original formatting and line breaks.
/// </remarks>
[PublicAPI]
public sealed record TextReportData(string Text) : ICustomReportData
{
    /// <summary>
    ///     Gets or initializes an optional title to display above the text content.
    /// </summary>
    /// <value>The text title, or <c>null</c> if no title should be displayed.</value>
    public string? Title { get; init; }

    /// <summary>
    ///     Gets or initializes a value indicating whether this text should appear before standard report data.
    /// </summary>
    /// <value><c>true</c> to position before logs and artifacts; <c>false</c> to position after.</value>
    public bool BeforeStandardData { get; init; }
}

/// <summary>
///     Specifies the horizontal alignment for table columns in <see cref="TableReportData" />.
/// </summary>
[PublicAPI]
public enum ColumnAlignment
{
    /// <summary>
    ///     Align content to the left side of the column.
    ///     This is the default alignment for most content types.
    /// </summary>
    Left,

    /// <summary>
    ///     Center content within the column.
    ///     Useful for headers or status indicators.
    /// </summary>
    Center,

    /// <summary>
    ///     Align content to the right side of the column.
    ///     Commonly used for numeric data or measurements.
    /// </summary>
    Right,
}
