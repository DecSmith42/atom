namespace DecSm.Atom.Util;

public sealed record AbsolutePath(IFileSystem FileSystem, string Path)
{
    public static AbsolutePath operator /(AbsolutePath left, string right) =>
        left with { Path = left.FileSystem.Path.Combine(left.Path, right) };

    public static implicit operator string(AbsolutePath path) =>
        path.Path;
}