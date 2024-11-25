namespace DecSm.Atom.Paths;

[PublicAPI]
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
