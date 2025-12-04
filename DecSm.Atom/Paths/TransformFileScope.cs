namespace DecSm.Atom.Paths;

/// <summary>
///     Represents a disposable scope for performing transformations on a file's content.
///     Restores the file's original content upon disposal (unless restoration has been explicitly canceled).
/// </summary>
/// <remarks>
///     Atom build uses this to set additional build properties in a project file during a build without permanently
///     modifying the file.
/// </remarks>
/// <example>
///     <code lang="csharp">
/// using (var scope = await TransformFileScope.CreateAsync(file, content => content + "\n<NewProperty>Value</NewProperty>"))
/// {
///     // Perform operations within the scope
///     // The file's content is transformed, and the original content will be restored upon disposal
/// }
/// </code>
/// </example>
[PublicAPI]
public sealed class TransformFileScope : IAsyncDisposable, IDisposable
{
    private readonly RootedPath _file;
    private readonly string? _initialContent;
    private bool _cancelled;
    private bool _disposed;

    private TransformFileScope(RootedPath file, string? initialContent)
    {
        _file = file;
        _initialContent = initialContent;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_cancelled)
            return;

        // No cancellation token here - we'd prefer to wait for the file to write so it doesn't get mangled.
        if (_initialContent is null)
            _file.FileSystem.File.Delete(_file);
        else
            await _file.FileSystem.File.WriteAllTextAsync(_file, _initialContent);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_cancelled)
            return;

        if (_initialContent is null)
            _file.FileSystem.File.Delete(_file);
        else
            _file.FileSystem.File.WriteAllText(_file, _initialContent);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="TransformFileScope" /> for the specified file,
    ///     applying the provided transformation to its content.
    /// </summary>
    /// <param name="file">The file to be managed by the scope.</param>
    /// <param name="transform">
    ///     A function that takes the initial content of the file as a string
    ///     and returns the transformed content as a string.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A task that represents the asynchronous operation and contains a <see cref="TransformFileScope" /> instance
    ///     managing the file.
    /// </returns>
    public static async Task<TransformFileScope> CreateAsync(
        RootedPath file,
        Func<string, string> transform,
        CancellationToken cancellationToken = default)
    {
        string? initialContent = null;

        if (!file.FileSystem.File.Exists(file))
            await file
                .FileSystem
                .File
                .Create(file)
                .DisposeAsync();
        else
            initialContent = await file.FileSystem.File.ReadAllTextAsync(file, cancellationToken);

        var scope = new TransformFileScope(file, initialContent);

        try
        {
            await file.FileSystem.File.WriteAllTextAsync(file,
                transform(initialContent ?? string.Empty),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await scope.DisposeAsync();

            throw;
        }

        return scope;
    }

    /// <summary>
    ///     Applies a transformation function to the current content of the file managed by the
    ///     <see cref="TransformFileScope" /> and updates its
    ///     content asynchronously.
    /// </summary>
    /// <param name="transform">
    ///     A function that takes the current content of the file as a string
    ///     and returns the updated content as a string.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A task that represents the asynchronous operation and contains the current <see cref="TransformFileScope" />
    ///     instance.
    /// </returns>
    public async Task<TransformFileScope> AddAsync(
        Func<string, string> transform,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cancelled)
            return this;

        try
        {
            var currentContent = await _file.FileSystem.File.ReadAllTextAsync(_file, cancellationToken);
            await _file.FileSystem.File.WriteAllTextAsync(_file, transform(currentContent), cancellationToken);

            return this;
        }
        catch (OperationCanceledException)
        {
            await DisposeAsync();

            throw;
        }
    }

    /// <summary>
    ///     Creates a new instance of <see cref="TransformFileScope" /> for the specified file,
    ///     applying the provided transformation to its content.
    /// </summary>
    /// <param name="file">The file to be managed by the scope.</param>
    /// <param name="transform">
    ///     A function that takes the initial content of the file as a string
    ///     and returns the transformed content as a string.
    /// </param>
    /// <returns>A <see cref="TransformFileScope" /> instance managing the file.</returns>
    public static TransformFileScope Create(RootedPath file, Func<string, string> transform)
    {
        string? initialContent = null;

        if (!file.FileSystem.File.Exists(file))
            file
                .FileSystem
                .File
                .Create(file)
                .Dispose();
        else
            initialContent = file.FileSystem.File.ReadAllText(file);

        var scope = new TransformFileScope(file, initialContent);

        file.FileSystem.File.WriteAllText(file, transform(initialContent ?? string.Empty));

        return scope;
    }

    /// <summary>
    ///     Modifies the content of the file managed by the current <see cref="TransformFileScope" /> instance,
    ///     applying the provided transformation function to its content.
    /// </summary>
    /// <param name="transform">
    ///     A function that takes the current content of the file as a string
    ///     and returns the modified content as a string.
    /// </param>
    /// <returns>
    ///     The current <see cref="TransformFileScope" /> instance with the updated file content.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown if the method is called after the <see cref="TransformFileScope" /> instance has been disposed.
    /// </exception>
    public TransformFileScope Add(Func<string, string> transform)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cancelled)
            return this;

        var currentContent = _file.FileSystem.File.ReadAllText(_file);
        _file.FileSystem.File.WriteAllText(_file, transform(currentContent));

        return this;
    }

    /// <summary>
    ///     Cancels the restoration of the original content of the file managed by this scope.
    ///     When called, the scope will not restore the original file content upon disposal or asynchronous disposal.
    /// </summary>
    public void CancelRestore() =>
        _cancelled = true;
}
