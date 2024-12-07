namespace DecSm.Atom.Tests.ClassTests.Build;

[TestFixture]
public class AtomBuildVersionProviderTests
{
    private static readonly string OsAgnosticRoot = OperatingSystem.IsWindows()
        ? @"C:\Solution"
        : "/Solution";

    private static readonly char Ps = Path.DirectorySeparatorChar;

    private static AtomFileSystem NewFileSystem(IFileSystem fileSystem)
    {
        var result = new AtomFileSystem
        {
            FileSystem = fileSystem,
            PathLocators = [],
            ProjectName = "Atom",
        };

        result.ClearCache();

        return result;
    }

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
        var fileSystem = NewFileSystem(new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $"{OsAgnosticRoot}{Ps}Solution.sln", new("<!-- -->") },
                { $"{OsAgnosticRoot}{Ps}Project", new MockDirectoryData() },
                { $"{OsAgnosticRoot}{Ps}Directory.Build.props", new(directoryBuildProps) },
            },
            OsAgnosticRoot));

        var provider = new AtomBuildVersionProvider(fileSystem);

        // Act
        var version = provider.Version;

        // Assert
        version.ShouldSatisfyAllConditions(x => x.ShouldNotBeNull(),
            x => x
                .ToString()
                .ShouldBe("1.2.3"));
    }

    [Test]
    [NonParallelizable]
    [SuppressMessage("ReSharper", "MoveLocalFunctionAfterJumpStatement")]
    public void Version_WhenDirectoryBuildPropsDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange

        var fileSystem = NewFileSystem(new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $"{OsAgnosticRoot}{Ps}Solution.sln", new("<!-- -->") },
                { $"{OsAgnosticRoot}{Ps}Project", new MockDirectoryData() },
            },
            OsAgnosticRoot));

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
