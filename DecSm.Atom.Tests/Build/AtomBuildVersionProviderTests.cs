namespace DecSm.Atom.Tests.Build;

[TestFixture]
public class AtomBuildVersionProviderTests
{
    private static readonly string OsAgnosticRoot = OperatingSystem.IsWindows()
        ? @"C:\Solution"
        : "/Solution";

    private static readonly char Ps = Path.DirectorySeparatorChar;

    [Test]
    public void Version_Returns_VersionInfo()
    {
        const string directoryBuildProps = """
                                           <Project>
                                               <PropertyGroup>
                                               <Version>1.2.3</Version>
                                               </PropertyGroup>
                                           </Project>
                                           """;

        // Arrange
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $"{OsAgnosticRoot}{Ps}Solution.sln", new MockFileData("<!-- -->") },
                { $"{OsAgnosticRoot}{Ps}Project", new MockDirectoryData() },
                { $"{OsAgnosticRoot}{Ps}Directory.Build.props", new MockFileData(directoryBuildProps) },
            },
            OsAgnosticRoot);

        var provider = new AtomBuildVersionProvider(fileSystem);

        // Act
        var version = provider.Version;

        // Assert
        version.ShouldSatisfyAllConditions(x => x.ShouldNotBeNull(),
            x => x
                .Version
                .ToString()
                .ShouldBe("1.2.3"));
    }

    [Test]
    [NonParallelizable]
    public void Version_WhenDirectoryBuildPropsDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        FileSystemExtensions.ClearCachedPaths();

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $"{OsAgnosticRoot}{Ps}Solution.sln", new MockFileData("<!-- -->") },
                { $"{OsAgnosticRoot}{Ps}Project", new MockDirectoryData() },
            },
            OsAgnosticRoot);

        var provider = new AtomBuildVersionProvider(fileSystem);

        // Act
        void Act()
        {
            _ = provider.Version;
        }

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }
}