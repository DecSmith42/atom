namespace DecSm.Atom.Reports;

public interface IReportsHelper : IBuildAccessor
{
    /// <summary>
    ///     Adds report data to the build process, which can be used for generating reports or logs.
    ///     If running locally, the report will be output to the console.
    ///     If running as part of a CI/CD pipeline, the report may be attached to the CI/CD run summary.
    /// </summary>
    /// <param name="reportData">The report data to add.</param>
    void AddReportData(IReportData reportData) =>
        GetService<ReportService>()
            .AddReportData(reportData);
}
