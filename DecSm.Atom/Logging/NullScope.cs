namespace DecSm.Atom.Logging;

internal sealed class NullScope : IDisposable
{
    internal static NullScope Instance { get; } = new();
    
    private NullScope() { }
    
    public void Dispose() { }
}