namespace DecSm.Atom.Paths;

[PublicAPI]
public interface ITransformFileScope : IAsyncDisposable, IDisposable
{
    public static async Task<ITransformFileScope> CreateAsync(AbsolutePath file, Func<string, string> transform)
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

    public static ITransformFileScope Create(AbsolutePath file, Func<string, string> transform)
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

    void CancelRestore();
}

internal class TransformFileScope : ITransformFileScope
{
    private readonly AbsolutePath _file;
    private readonly string? _initialContent;
    private bool _cancelled;
    private bool _disposed;

    internal TransformFileScope(AbsolutePath file, string? initialContent)
    {
        _file = file;
        _initialContent = initialContent;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (!_cancelled)
            if (_initialContent is null)
                _file.FileSystem.File.Delete(_file);
            else
                _file.FileSystem.File.WriteAllText(_file, _initialContent);

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (!_cancelled)
            if (_initialContent is null)
                _file.FileSystem.File.Delete(_file);
            else
                await _file.FileSystem.File.WriteAllTextAsync(_file, _initialContent);

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public void CancelRestore() =>
        _cancelled = true;
}
