namespace DecSm.Atom.Reports;

public interface IReportData;

public interface ICustomReportData : IReportData
{
    bool BeforeStandardData { get; }
}

public sealed record LogReportData(string Message, Exception? Exception, LogLevel Level, DateTimeOffset Timestamp) : IReportData;

public sealed record ArtifactReportData(string Name, string Path) : IReportData;

public sealed record TableReportData(IReadOnlyList<IReadOnlyList<string>> Rows) : ICustomReportData
{
    public string? Title { get; init; }

    public IReadOnlyList<string>? Header { get; init; }

    public IReadOnlyList<ColumnAlignment> ColumnAlignments { get; init; } = [];

    public bool BeforeStandardData { get; init; }
}

public sealed record ListReportData(IReadOnlyList<string> Items) : ICustomReportData
{
    public string? Title { get; init; }

    public string Prefix { get; init; } = "- ";

    public bool BeforeStandardData { get; init; }
}

public sealed record TextReportData(string Text) : ICustomReportData
{
    public string? Title { get; init; }

    public bool BeforeStandardData { get; init; }
}

public enum ColumnAlignment
{
    Left,
    Center,
    Right,
}
