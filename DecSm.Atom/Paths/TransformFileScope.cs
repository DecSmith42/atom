namespace DecSm.Atom.Paths;

[PublicAPI]
public sealed class TransformFileScope : IAsyncDisposable, IDisposable
{
    private readonly AbsolutePath _file;
    private readonly string? _initialContent;
    private bool _cancelled;
    private bool _disposed;

    private TransformFileScope(AbsolutePath file, string? initialContent)
    {
        _file = file;
        _initialContent = initialContent;
    }

    public static async Task<TransformFileScope> CreateAsync(AbsolutePath file, Func<string, string> transform)
    {
        string? initialContent = null;

        if (!file.FileSystem.File.Exists(file))
            await file
                .FileSystem
                .File
                .Create(file)
                .DisposeAsync();
        else
            initialContent = await file.FileSystem.File.ReadAllTextAsync(file);

        var scope = new TransformFileScope(file, initialContent);

        await file.FileSystem.File.WriteAllTextAsync(file, transform(initialContent ?? string.Empty));

        return scope;
    }

    public static TransformFileScope Create(AbsolutePath file, Func<string, string> transform)
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

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_cancelled)
            return;

        if (_initialContent is null)
            _file.FileSystem.File.Delete(_file);
        else
            await _file.FileSystem.File.WriteAllTextAsync(_file, _initialContent);
    }

    public void CancelRestore() =>
        _cancelled = true;
}
