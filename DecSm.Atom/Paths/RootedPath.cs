namespace DecSm.Atom.Paths;

/// <summary>
///     A rooted path in the file system.
///     Implicitly convertible to a string.
/// </summary>
[PublicAPI]
public sealed record RootedPath(IAtomFileSystem FileSystem, string Path)
{
    /// <summary>
    ///     Gets the parent directory of the current path if it exists.
    /// </summary>
    /// <remarks>
    ///     The <c>Parent</c> property calculates the parent directory of the current path by trimming the last segment of the path.
    ///     This property handles both forward slashes ("/") and backslashes ("\") as path separators,
    ///     retaining compatibility with various file system conventions.
    /// </remarks>
    public RootedPath? Parent
    {
        get
        {
            if (FileSystem.Path.GetPathRoot(Path) == Path)
                return null;

            var path = Path switch
            {
                [.., '/'] or [.., '\\'] => Path[..^1],
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

    /// <summary>
    ///     Indicates whether the current path exists in the file system as either a file or a directory.
    /// </summary>
    /// <remarks>
    ///     The <c>PathExists</c> property returns <c>true</c> if either <c>FileExists</c> or <c>DirectoryExists</c> evaluates to <c>true</c>,
    ///     ensuring that the path is valid and points to an existing entity in the file system.
    /// </remarks>
    public bool PathExists => FileExists || DirectoryExists;

    /// <summary>
    ///     Indicates whether the current path exists in the file system as a file.
    /// </summary>
    public bool FileExists => FileSystem.File.Exists(Path);

    /// <summary>
    ///     Indicates whether the current path exists in the file system as a directory.
    /// </summary>
    public bool DirectoryExists => FileSystem.Directory.Exists(Path);

    /// <summary>
    ///     Gets the file name from the current path if the file exists.
    /// </summary>
    /// <remarks>
    ///     The <c>FileName</c> property retrieves the name of the file from the specified path using the file system's path utilities.
    /// </remarks>
    public string? FileName =>
        FileExists
            ? FileSystem.Path.GetFileName(Path)
            : null;

    /// <summary>
    ///     Gets the directory name of the current path if it exists and represents a directory.
    /// </summary>
    /// <remarks>
    ///     The <c>DirectoryName</c> property retrieves the name of the directory that the current path points to
    ///     if the path represents an existing directory.
    /// </remarks>
    public string? DirectoryName =>
        DirectoryExists
            ? FileSystem.Path.GetDirectoryName(Path)
            : null;

    /// <summary>
    ///     Defines a custom operator for combining a <see cref="RootedPath" /> with a string.
    /// </summary>
    /// <param name="left">The original <see cref="RootedPath" /> instance.</param>
    /// <param name="right">The string to combine with the path of the <see cref="RootedPath" />.</param>
    /// <remarks>
    ///     This operator uses <see cref="Path.Combine(string, string)" /> to combine the original path with the provided string.
    /// </remarks>
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
