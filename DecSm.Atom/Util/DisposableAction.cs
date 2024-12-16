namespace DecSm.Atom.Util;

[PublicAPI]
public sealed record DisposableAction(Action? Action = null) : IDisposable
{
    public void Dispose() =>
        Action?.Invoke();
}
