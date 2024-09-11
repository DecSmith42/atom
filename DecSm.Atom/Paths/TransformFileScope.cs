namespace DecSm.Atom.Paths;

public interface ITransformFileScope : IAsyncDisposable, IDisposable
{
    public static async Task<ITransformFileScope> CreateAsync(AbsolutePath file, Func<string, string> transform)
    {
        var initialContent = await file.FileSystem.File.ReadAllTextAsync(file);

        var scope = new TransformFileScope(file, initialContent);

        await file.FileSystem.File.WriteAllTextAsync(file, transform(initialContent));

        return scope;
    }

    public static ITransformFileScope Create(AbsolutePath file, Func<string, string> transform)
    {
        var initialContent = file.FileSystem.File.ReadAllText(file);

        var scope = new TransformFileScope(file, initialContent);

        file.FileSystem.File.WriteAllText(file, transform(initialContent));

        return scope;
    }
}

internal class TransformFileScope : ITransformFileScope
{
    private readonly AbsolutePath _file;
    private readonly string? _initialContent;
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

        _file.FileSystem.File.WriteAllText(_file, _initialContent);

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await _file.FileSystem.File.WriteAllTextAsync(_file, _initialContent);

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
