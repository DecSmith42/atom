namespace DecSm.Atom.Util.Scope;

/// <summary>
///     A disposable wrapper that executes an action when disposed.
///     Useful for implementing RAII (Resource Acquisition Is Initialization) patterns
///     and ensuring cleanup code runs in using statements or try-finally blocks.
/// </summary>
/// <param name="OnDispose">The action to execute when the object is disposed. Can be null.</param>
[PublicAPI]
public sealed record ActionScope(Action? OnDispose = null) : IDisposable
{
    /// <summary>
    ///     Executes the wrapped action if it is not null.
    /// </summary>
    public void Dispose() =>
        OnDispose?.Invoke();
}
