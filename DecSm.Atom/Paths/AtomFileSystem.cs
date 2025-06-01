namespace DecSm.Atom.Paths;

/// <summary>
///     Represents the abstraction layer over the file system specifically tailored for Atom,
///     encapsulating paths and directories relevant to the Atom build automation environment.
///     This interface extends from the general <see cref="IFileSystem" /> abstraction.
/// </summary>
[PublicAPI]
public interface IAtomFileSystem : IFileSystem
{
    /// <summary>
    ///     Gets the name of the project associated with the Atom file system,
    ///     typically the name of the entry assembly.
    /// </summary>
    string ProjectName { get; }

    /// <summary>
    ///     Gets the underlying <see cref="IFileSystem" /> instance providing
    ///     general file system functionality used internally.
    /// </summary>
    public IFileSystem FileSystem { get; }

    /// <summary>
    ///     Gets the root directory of the Atom project.
    ///     Defined by locating a directory containing project markers (e.g., *.git, *.sln).
    /// </summary>
    RootedPath AtomRootDirectory => GetPath(AtomPaths.Root);

    /// <summary>
    ///     Gets the directory where Atom build artifacts are stored by default.
    /// </summary>
    RootedPath AtomArtifactsDirectory => GetPath(AtomPaths.Artifacts);

    /// <summary>
    ///     Gets the default directory where Atom publishes final outputs.
    /// </summary>
    RootedPath AtomPublishDirectory => GetPath(AtomPaths.Publish);

    /// <summary>
    ///     Gets the temporary working directory for Atom tasks and operations.
    /// </summary>
    RootedPath AtomTempDirectory => GetPath(AtomPaths.Temp);

    /// <summary>
    ///     Gets the current working directory of the running process.
    /// </summary>
    RootedPath CurrentDirectory => new(this, FileSystem.Directory.GetCurrentDirectory());

    IDirectory IFileSystem.Directory => FileSystem.Directory;

    IDirectoryInfoFactory IFileSystem.DirectoryInfo => FileSystem.DirectoryInfo;

    IDriveInfoFactory IFileSystem.DriveInfo => FileSystem.DriveInfo;

    IFile IFileSystem.File => FileSystem.File;

    IFileInfoFactory IFileSystem.FileInfo => FileSystem.FileInfo;

    IFileStreamFactory IFileSystem.FileStream => FileSystem.FileStream;

    IFileSystemWatcherFactory IFileSystem.FileSystemWatcher => FileSystem.FileSystemWatcher;

    IPath IFileSystem.Path => FileSystem.Path;

    IFileVersionInfoFactory IFileSystem.FileVersionInfo => FileSystem.FileVersionInfo;

    /// <summary>
    ///     Retrieves or calculates a path based on the provided Atom path key.
    /// </summary>
    /// <param name="key">The key identifying the path within Atom context.</param>
    /// <returns>A <see cref="RootedPath" /> instance corresponding to the key.</returns>
    RootedPath GetPath(string key);

    /// <summary>
    ///     Creates a new <see cref="RootedPath" /> based on the specified path string.
    /// </summary>
    /// <param name="path">The string representation of the path.</param>
    /// <returns>A new rooted path using the Atom file system instance.</returns>
    RootedPath CreateRootedPath(string path) =>
        new(this, path);
}

internal sealed class AtomFileSystem : IAtomFileSystem
{
    private readonly Dictionary<string, RootedPath> _pathCache = [];

    public required IReadOnlyList<IPathProvider> PathLocators { private get; init; }

    public string ProjectName { get; init; } = Assembly.GetEntryAssembly()!.GetName()
        .Name!;

    public required IFileSystem FileSystem { get; init; }

    public RootedPath GetPath(string key)
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

    private RootedPath GetRoot()
    {
        var currentDir = ((IAtomFileSystem)this).CurrentDirectory;

        while (currentDir.Parent is not null)
        {
            currentDir = currentDir.Parent;

            if (FileSystem
                    .Directory
                    .EnumerateDirectories(currentDir, ProjectName, SearchOption.TopDirectoryOnly)
                    .Any() ||
                FileSystem
                    .Directory
                    .EnumerateDirectories(currentDir, "*.git")
                    .Any() ||
                FileSystem
                    .Directory
                    .EnumerateDirectories(currentDir, "*.sln")
                    .Any())
                return currentDir;
        }

        return ((IAtomFileSystem)this).CurrentDirectory;
    }

    private RootedPath GetArtifacts() =>
        GetPath(AtomPaths.Root) / "atom-publish";

    private RootedPath GetPublish() =>
        GetPath(AtomPaths.Root) / "atom-publish";

    private RootedPath GetTemp() =>
        new(this, FileSystem.Path.GetTempPath());
}
