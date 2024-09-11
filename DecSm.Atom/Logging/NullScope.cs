namespace DecSm.Atom.Logging;

internal sealed class NullScope : IDisposable
{
    private NullScope() { }

    internal static NullScope Instance { get; } = new();

    public void Dispose() { }
}
