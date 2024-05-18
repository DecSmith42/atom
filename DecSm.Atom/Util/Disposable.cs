namespace DecSm.Atom.Util;

public sealed record Disposable(Action Action) : IDisposable
{
    public void Dispose() =>
        Action();
}