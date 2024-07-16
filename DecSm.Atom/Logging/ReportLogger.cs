namespace DecSm.Atom.Logging;

public class ReportLogger(string categoryName, IExternalScopeProvider? scopeProvider) : ILogger
{
    public bool IsEnabled(LogLevel logLevel) =>
        logLevel != LogLevel.None;

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull =>
        scopeProvider?.Push(state) ?? NullScope.Instance;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        if (logLevel is LogLevel.Debug or LogLevel.Trace or LogLevel.Information)
            return;

        var time = DateTimeOffset.Now;
        string? command = null;

        scopeProvider?.ForEachScope((scopeData, _) =>
            {
                if (scopeData is not Dictionary<string, object> scopeValues)
                    return;

                if (scopeValues.GetValueOrDefault("Command") is string currentCommand)
                    command = currentCommand;
            },
            state);

        var message = formatter(state, exception);

        if (message is "(null)")
            return;

        // If the message contains any secrets, we don't want to log it
        message = ParamServiceAccessor.Service?.MaskSecrets(message) ?? message;

        ReportServiceAccessor.Service?.AddReportData(new LogReportData(message, exception, logLevel, time), command);
    }
}