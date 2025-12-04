namespace DecSm.Atom.Util.Scope;

internal readonly struct NullScope : IDisposable
{
    public static readonly NullScope Instance = new();

    public void Dispose() { }
}
