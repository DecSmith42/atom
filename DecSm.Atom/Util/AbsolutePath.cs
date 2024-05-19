namespace DecSm.Atom.Util;

public sealed record AbsolutePath(IFileSystem FileSystem, string Path)
{
    public static AbsolutePath operator /(AbsolutePath left, string right) =>
        left with
        {
            Path = left.FileSystem.Path.Combine(left.Path, right),
        };
    
    public static implicit operator string(AbsolutePath path) =>
        path.Path;
    
    public static AbsolutePath FromFileInfo(IFileInfo fileInfo) =>
        new(fileInfo.FileSystem, fileInfo.FullName);
    
    public AbsolutePath? Parent
    {
        get
        {
            var parent = FileSystem.Path.GetDirectoryName(Path);
            
            if (parent == null)
                return null;
            
            return this with
            {
                Path = parent,
            };
        }
    }
    
    public override string ToString() =>
        Path;
    
    public bool Exists => FileSystem.File.Exists(Path) || FileSystem.Directory.Exists(Path);
    
    public string? FileName =>
        FileSystem.File.Exists(Path)
            ? FileSystem.Path.GetFileName(Path)
            : null;
    
    public string? FileExtension =>
        FileSystem.File.Exists(Path)
            ? FileSystem.Path.GetExtension(Path)
            : null;
}