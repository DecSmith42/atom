namespace DecSm.Atom.Reports;

public interface IReportService
{
    void AddReportData(IReportData reportData, string? targetName = null);

    List<IReportData> GetReportData();
}