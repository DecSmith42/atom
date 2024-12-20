﻿namespace DecSm.Atom.Paths;

[PublicAPI]
public sealed record RootedPath(IAtomFileSystem FileSystem, string Path)
{
    public RootedPath? Parent
    {
        get
        {
            if (FileSystem.Path.GetPathRoot(Path) == Path)
                return null;

            var path = Path switch
            {
                [.., '/'] => Path[..^1],
                [.., '\\'] => Path[..^1],
                _ => Path,
            };

            var lastForwardSlash = path.LastIndexOf('/');
            var lastBackSlash = path.LastIndexOf('\\');

            var lastSlash = Math.Max(lastForwardSlash, lastBackSlash);

            if (lastSlash == -1)
                return null;

            return this with
            {
                Path = $"{path[..lastSlash]}{FileSystem.Path.DirectorySeparatorChar}",
            };
        }
    }

    public bool PathExists => FileExists || DirectoryExists;

    public bool FileExists => FileSystem.File.Exists(Path);

    public bool DirectoryExists => FileSystem.Directory.Exists(Path);

    public string? FileName =>
        FileExists
            ? FileSystem.Path.GetFileName(Path)
            : null;

    public string? DirectoryName =>
        DirectoryExists
            ? FileSystem.Path.GetDirectoryName(Path)
            : null;

    public static RootedPath operator /(RootedPath left, string right) =>
        left with
        {
            Path = left.FileSystem.Path.Combine(left.Path, right),
        };

    public static implicit operator string(RootedPath path) =>
        path.Path;

    public override string ToString() =>
        Path;
}
