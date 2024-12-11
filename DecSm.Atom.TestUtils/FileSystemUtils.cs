namespace DecSm.Atom.TestUtils;

public static class FileSystemUtils
{
    public static MockFileSystem DefaultMockFileSystem =>
        Environment.OSVersion.Platform is PlatformID.Win32NT
            ? new(new Dictionary<string, MockFileData>
                {
                    { @"C:\Atom\Atom.sln", new(string.Empty) },
                    { @"C:\Atom\_atom\_atom.csproj", new(string.Empty) },
                },
                @"C:\Atom\_atom")
            : new(new Dictionary<string, MockFileData>
                {
                    { "/Atom/Atom.sln", new(string.Empty) },
                    { "/Atom/_atom/_atom.csproj", new(string.Empty) },
                },
                "/Atom/_atom");
}
