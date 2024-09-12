namespace DecSm.Atom.Util;

public sealed record DisposableAction(Action Action) : IDisposable
{
    public void Dispose() =>
        Action();
}
