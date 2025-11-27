namespace DecSm.Atom.Paths;

/// <summary>
///     Similar to <see cref="TransformFileScope" />, but for multiple files.
/// </summary>
[PublicAPI]
public sealed class TransformMultiFileScope : IAsyncDisposable, IDisposable
{
    private readonly IEnumerable<RootedPath> _files;
    private readonly string?[] _initialContents;
    private bool _cancelled;
    private bool _disposed;

    private TransformMultiFileScope(IEnumerable<RootedPath> files, string?[] initialContents)
    {
        _files = files;
        _initialContents = initialContents;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_cancelled)
            return;

        await Task.WhenAll(_files.Select(async (x, i) =>
        {
            if (_initialContents[i] is null)
                x.FileSystem.File.Delete(x);
            else
                await x.FileSystem.File.WriteAllTextAsync(x, _initialContents[i]);
        }));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_cancelled)
            return;

        // No cancellation token here - we'd prefer to wait for the files to write so they don't get mangled.
        foreach (var (file, i) in _files.Select((x, i) => (x, i)))
            if (_initialContents[i] is null)
                file.FileSystem.File.Delete(file);
            else
                file.FileSystem.File.WriteAllText(file, _initialContents[i]);
    }

    /// <summary>
    ///     See <see cref="TransformFileScope.CreateAsync(RootedPath, Func{string, string}, CancellationToken)" />
    /// </summary>
    public static async Task<TransformMultiFileScope> CreateAsync(
        IEnumerable<RootedPath> files,
        Func<string, string> transform,
        CancellationToken cancellationToken = default)
    {
        var filesArray = files.ToArray();

        var initialContents = await Task.WhenAll(filesArray.Select(async x =>
        {
            if (x.FileSystem.File.Exists(x))
                return await x.FileSystem.File.ReadAllTextAsync(x, cancellationToken);

            await x
                .FileSystem
                .File
                .Create(x)
                .DisposeAsync();

            return null;
        }));

        var scope = new TransformMultiFileScope(filesArray, initialContents);

        try
        {
            await Task.WhenAll(filesArray.Select((x, i) =>
                x.FileSystem.File.WriteAllTextAsync(x,
                    transform(initialContents[i] ?? string.Empty),
                    cancellationToken)));
        }
        catch (OperationCanceledException)
        {
            await scope.DisposeAsync();
        }

        return scope;
    }

    /// <summary>
    ///     See <see cref="TransformFileScope.AddAsync(Func{string, string},CancellationToken)" />
    /// </summary>
    public async Task<TransformMultiFileScope> AddAsync(
        Func<string, string> transform,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cancelled)
            return this;

        try
        {
            await Task.WhenAll(_files.Select(async x =>
            {
                var currentContent = await x.FileSystem.File.ReadAllTextAsync(x, cancellationToken);
                await x.FileSystem.File.WriteAllTextAsync(x, transform(currentContent), cancellationToken);
            }));

            return this;
        }
        catch (OperationCanceledException)
        {
            await DisposeAsync();

            throw;
        }
    }

    /// <summary>
    ///     Creates a new <see cref="TransformMultiFileScope" /> for the given files.
    /// </summary>
    public static TransformMultiFileScope Create(IEnumerable<RootedPath> files, Func<string, string> transform)
    {
        var filesArray = files.ToArray();

        var initialContents = filesArray
            .Select(x =>
            {
                if (x.FileSystem.File.Exists(x))
                    return x.FileSystem.File.ReadAllText(x);

                x
                    .FileSystem
                    .File
                    .Create(x)
                    .Dispose();

                return null;
            })
            .ToArray();

        var scope = new TransformMultiFileScope(filesArray, initialContents);

        foreach (var (file, i) in filesArray.Select((x, i) => (x, i)))
            file.FileSystem.File.WriteAllText(file, transform(initialContents[i] ?? string.Empty));

        return scope;
    }

    /// <summary>
    ///     See <see cref="TransformFileScope.Add(Func{string, string})" />
    /// </summary>
    public TransformMultiFileScope Add(Func<string, string> transform)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cancelled)
            return this;

        foreach (var file in _files)
        {
            var currentContent = file.FileSystem.File.ReadAllText(file);
            file.FileSystem.File.WriteAllText(file, transform(currentContent));
        }

        return this;
    }

    /// <summary>
    ///     Cancels the restore operation.
    /// </summary>
    public void CancelRestore() =>
        _cancelled = true;
}
