namespace DecSm.Atom.Paths;

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

public interface ITransformMultiFileScope : IAsyncDisposable, IDisposable
{
    public static async Task<ITransformMultiFileScope> CreateAsync(IEnumerable<AbsolutePath> files, Func<string, string> transform)
    {
        var filesArray = files.ToArray();

        var initialContents = await Task.WhenAll(filesArray.Select(async x =>
        {
            if (x.FileSystem.File.Exists(x))
                return await x.FileSystem.File.ReadAllTextAsync(x);

            await x
                .FileSystem
                .File
                .Create(x)
                .DisposeAsync();

            return null;
        }));

        var scope = new TransformMultiFileScope(filesArray, initialContents);

        await Task.WhenAll(
            filesArray.Select((x, i) => x.FileSystem.File.WriteAllTextAsync(x, transform(initialContents[i] ?? string.Empty))));

        return scope;
    }

    public static ITransformMultiFileScope Create(IEnumerable<AbsolutePath> files, Func<string, string> transform)
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

    void CancelRestore();
}

internal class TransformMultiFileScope : ITransformMultiFileScope
{
    private readonly IEnumerable<AbsolutePath> _files;
    private readonly string?[] _initialContents;
    private bool _cancelled;
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

        if (!_cancelled)
            foreach (var (file, i) in _files.Select((x, i) => (x, i)))
                if (_initialContents[i] is null)
                    file.FileSystem.File.Delete(file);
                else
                    file.FileSystem.File.WriteAllText(file, _initialContents[i]);

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (!_cancelled)
            await Task.WhenAll(_files.Select(async (x, i) =>
            {
                if (_initialContents[i] is null)
                    x.FileSystem.File.Delete(x);
                else
                    await x.FileSystem.File.WriteAllTextAsync(x, _initialContents[i]);
            }));

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public void CancelRestore() =>
        _cancelled = true;
}
