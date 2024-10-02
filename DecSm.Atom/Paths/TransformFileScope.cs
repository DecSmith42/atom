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

public interface ITransformMultiFileScope : IAsyncDisposable, IDisposable
{
    public static async Task<ITransformMultiFileScope> CreateAsync(IEnumerable<AbsolutePath> files, Func<string, string> transform)
    {
        var initialContents = await Task.WhenAll(files.Select(x => x.FileSystem.File.ReadAllTextAsync(x)));

        var scope = new TransformMultiFileScope(files, initialContents);

        await Task.WhenAll(files.Select((x, i) => x.FileSystem.File.WriteAllTextAsync(x, transform(initialContents[i]))));

        return scope;
    }

    public static ITransformMultiFileScope Create(IEnumerable<AbsolutePath> files, Func<string, string> transform)
    {
        var initialContents = files.Select(x => x.FileSystem.File.ReadAllText(x)).ToArray();

        var scope = new TransformMultiFileScope(files, initialContents);

        foreach (var (file, i) in files.Select((x, i) => (x, i)))
            file.FileSystem.File.WriteAllText(file, transform(initialContents[i]));

        return scope;
    }
}

internal class TransformMultiFileScope : ITransformMultiFileScope
{
    private readonly IEnumerable<AbsolutePath> _files;
    private readonly string?[] _initialContents;
    private bool _disposed;

    internal TransformMultiFileScope(IEnumerable<AbsolutePath> files, string?[] initialContents)
    {
        _files = files;
        _initialContents = initialContents;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var (file, i) in _files.Select((x, i) => (x, i)))
            file.FileSystem.File.WriteAllText(file, _initialContents[i]);

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await Task.WhenAll(_files.Select((x, i) => x.FileSystem.File.WriteAllTextAsync(x, _initialContents[i])));

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}