namespace DecSm.Atom.Reports;

public interface IReportData;

public interface ICustomReportData : IReportData
{
    bool BeforeStandardData { get; init; }
}

public sealed record LogReportData(string Message, Exception? Exception, LogLevel Level, DateTimeOffset Timestamp) : IReportData;

public sealed record ArtifactReportData(string Name, string Path) : IReportData;

public sealed record TextReportData(string Text) : ICustomReportData
{
    public string? Title { get; init; }

    public bool BeforeStandardData { get; init; }
}

public sealed record ListReportData(IReadOnlyList<string> Items) : ICustomReportData
{
    public string? Title { get; init; }

    public bool BeforeStandardData { get; init; }
}

public sealed record TableReportData(IReadOnlyList<IReadOnlyList<string>> Rows) : ICustomReportData
{
    public string? Title { get; init; }

    public IReadOnlyList<string>? Header { get; init; }

    public bool BeforeStandardData { get; init; }
}