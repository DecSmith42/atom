﻿namespace DecSm.Atom.Paths;

[PublicAPI]
public interface IAtomFileSystem : IFileSystem
{
    string ProjectName { get; }

    public IFileSystem FileSystem { get; }

    RootedPath AtomRootDirectory => GetPath(AtomPaths.Root);

    RootedPath AtomArtifactsDirectory => GetPath(AtomPaths.Artifacts);

    RootedPath AtomPublishDirectory => GetPath(AtomPaths.Publish);

    RootedPath AtomTempDirectory => GetPath(AtomPaths.Temp);

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

    RootedPath GetPath(string key);

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

    private RootedPath GetArtifacts() =>
        GetPath(AtomPaths.Root) / "atom-publish";

    private RootedPath GetPublish() =>
        GetPath(AtomPaths.Root) / "atom-publish";

    private RootedPath GetTemp() =>
        new(this, FileSystem.Path.GetTempPath());
}
