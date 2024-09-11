namespace DecSm.Atom.Process;

public sealed record ProcessRunOptions(string Name, string Args)
{
    public ProcessRunOptions(string Name, string[] Args) : this(Name, string.Join(" ", Args)) { }

    public string? WorkingDirectory { get; init; }

    public LogLevel InvocationLogLevel { get; init; } = LogLevel.Information;

    public LogLevel OutputLogLevel { get; init; } = LogLevel.Debug;

    public LogLevel ErrorLogLevel { get; init; } = LogLevel.Warning;

    public bool AllowFailedResult { get; init; }
}
