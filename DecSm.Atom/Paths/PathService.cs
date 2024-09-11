namespace DecSm.Atom.Paths;

public interface IAtomFileSystem : IFileSystem
{
    public IFileSystem FileSystem { get; }

    AbsolutePath AtomRootDirectory => GetPath(AtomPaths.Root);

    AbsolutePath AtomArtifactsDirectory => GetPath(AtomPaths.Artifacts);

    AbsolutePath AtomPublishDirectory => GetPath(AtomPaths.Publish);

    AbsolutePath AtomTempDirectory => GetPath(AtomPaths.Temp);

    AbsolutePath CurrentDirectory => new(this, FileSystem.Directory.GetCurrentDirectory());

    IDirectory IFileSystem.Directory => FileSystem.Directory;

    IDirectoryInfoFactory IFileSystem.DirectoryInfo => FileSystem.DirectoryInfo;

    IDriveInfoFactory IFileSystem.DriveInfo => FileSystem.DriveInfo;

    IFile IFileSystem.File => FileSystem.File;

    IFileInfoFactory IFileSystem.FileInfo => FileSystem.FileInfo;

    IFileStreamFactory IFileSystem.FileStream => FileSystem.FileStream;

    IFileSystemWatcherFactory IFileSystem.FileSystemWatcher => FileSystem.FileSystemWatcher;

    IPath IFileSystem.Path => FileSystem.Path;

    AbsolutePath GetPath(string key);
}

internal sealed class AtomFileSystem : IAtomFileSystem
{
    private readonly Dictionary<string, AbsolutePath> _pathCache = [];

    public required IReadOnlyList<IPathProvider> PathLocators { private get; init; }

    public required IFileSystem FileSystem { get; init; }

    public AbsolutePath GetPath(string key)
    {
        if (_pathCache.TryGetValue(key, out var path))
            return path;

        var locate = (string locatorKey) =>
        {
            if (locatorKey == key)
                throw new InvalidOperationException($"Locator for key '{key}' is circular");

            return GetPath(locatorKey);
        };

        path = PathLocators
            .Select(x => x.Locate(key, locate))
            .FirstOrDefault(x => x is not null);

        if (path is not null)
            return _pathCache[key] = path;

        return _pathCache[key] = key switch
        {
            AtomPaths.Root => GetRoot(),
            AtomPaths.Artifacts => GetArtifacts(),
            AtomPaths.Publish => GetPublish(),
            AtomPaths.Temp => GetTemp(),
            _ => throw new InvalidOperationException($"Could not locate path for key '{key}'"),
        };
    }

    internal void ClearCache() =>
        _pathCache.Clear();

    private AbsolutePath GetRoot()
    {
        var currentDir = ((IAtomFileSystem)this).CurrentDirectory;

        while (currentDir.Parent is not null)
        {
            currentDir = currentDir.Parent;

            if (FileSystem
                .Directory
                .EnumerateDirectories(currentDir, "_atom", SearchOption.TopDirectoryOnly)
                .Any())
                return currentDir;

            if (FileSystem
                .Directory
                .EnumerateDirectories(currentDir, "*.git")
                .Any())
                return currentDir;

            if (FileSystem
                .Directory
                .EnumerateDirectories(currentDir, "*.sln")
                .Any())
                return currentDir;
        }

        return ((IAtomFileSystem)this).CurrentDirectory;
    }

    private AbsolutePath GetArtifacts() =>
        GetPath(AtomPaths.Root) / "atom-publish";

    private AbsolutePath GetPublish() =>
        GetPath(AtomPaths.Root) / "atom-publish";

    private AbsolutePath GetTemp() =>
        new(this, FileSystem.Path.GetTempPath());
}
