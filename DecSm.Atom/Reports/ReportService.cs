namespace DecSm.Atom.Reports;

[PublicAPI]
public sealed class ReportService
{
    private readonly Dictionary<string, List<IReportData>> _reportData = [];

    public void AddReportData(IReportData reportData, string? targetName = null)
    {
        targetName ??= string.Empty;

        if (!_reportData.ContainsKey(targetName))
            _reportData[targetName] = [];

        _reportData[targetName]
            .Add(reportData);
    }

    public List<IReportData> GetReportData() =>
        _reportData
            .SelectMany(x => x.Value)
            .ToList();
}
