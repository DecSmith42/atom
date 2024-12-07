namespace DecSm.Atom.Tests.Utils;

public sealed class TestLoggerProvider : ILoggerProvider
{
    public TestLogger Logger { get; } = new();

    public void Dispose() { }

    public ILogger CreateLogger(string categoryName) =>
        Logger;
}
