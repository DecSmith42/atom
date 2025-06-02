namespace DecSm.Atom.Util;

/// <summary>
///     A disposable wrapper that executes an action when disposed.
///     Useful for implementing RAII (Resource Acquisition Is Initialization) patterns
///     and ensuring cleanup code runs in using statements or try-finally blocks.
/// </summary>
/// <param name="Action">The action to execute when the object is disposed. Can be null.</param>
[PublicAPI]
public sealed record DisposableAction(Action? Action = null) : IDisposable
{
    /// <summary>
    ///     Executes the wrapped action if it is not null.
    /// </summary>
    public void Dispose() =>
        Action?.Invoke();
}
