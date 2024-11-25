namespace DecSm.Atom.Util;

[PublicAPI]
public sealed record DisposableAction(Action Action) : IDisposable
{
    public void Dispose() =>
        Action();
}
