namespace DecSm.Atom.Reports;

/// <summary>
///     A service for collecting and managing report data from build targets and operations.
///     This service allows adding report data items and retrieving them for output formatting.
/// </summary>
[PublicAPI]
public sealed class ReportService
{
    private readonly Dictionary<string, List<IReportData>> _reportData = [];

    /// <summary>
    ///     Adds report data to the service, optionally associating it with a specific target.
    /// </summary>
    /// <param name="reportData">The report data to add. This can be any implementation of <see cref="IReportData" />.</param>
    /// <param name="targetName">
    ///     The name of the target this report data is associated with.
    ///     If null or not provided, the data will be stored under an empty string key.
    /// </param>
    /// <remarks>
    ///     Report data is grouped by target name internally. Multiple report data items
    ///     can be added for the same target name.
    /// </remarks>
    public void AddReportData(IReportData reportData, string? targetName = null)
    {
        targetName ??= string.Empty;

        if (!_reportData.ContainsKey(targetName))
            _reportData[targetName] = [];

        _reportData[targetName]
            .Add(reportData);
    }

    /// <summary>
    ///     Retrieves all report data items that have been added to the service.
    /// </summary>
    /// <returns>
    ///     A list containing all <see cref="IReportData" /> items from all targets,
    ///     flattened into a single collection.
    /// </returns>
    /// <remarks>
    ///     The returned list contains report data from all targets merged together.
    ///     The original target association is not preserved in the returned collection.
    /// </remarks>
    public List<IReportData> GetReportData() =>
        _reportData
            .SelectMany(x => x.Value)
            .ToList();
}
