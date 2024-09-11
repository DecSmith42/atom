namespace DecSm.Atom.Logging;

internal sealed class SpectreLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, SpectreLogger> _loggers = new();
    private readonly LoggerExternalScopeProvider _scopeProvider = new();

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, new SpectreLogger(categoryName, _scopeProvider));

    public void Dispose() =>
        _loggers.Clear();
}
