namespace DecSm.Atom.Logging;

internal sealed class ReportLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ReportLogger> _loggers = new();
    private readonly LoggerExternalScopeProvider _scopeProvider = new();

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, new ReportLogger(_scopeProvider));

    public void Dispose() =>
        _loggers.Clear();
}