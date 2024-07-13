using DecSm.Atom.Util;

namespace DecSm.Atom.Tests.Build;

[TestFixture]
public class AtomBuildVersionProviderTests
{
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
                { @"C:\Solution\Solution.sln", new MockFileData("<!-- -->") },
                { @"C:\Solution\Project", new MockDirectoryData() },
                { @"C:\Solution\Directory.Build.props", new MockFileData(directoryBuildProps) },
            },
            @"C:\Solution");

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
                { @"C:\Solution\Solution.sln", new MockFileData("<!-- -->") },
                { @"C:\Solution\Project", new MockDirectoryData() },
            },
            @"C:\Solution");

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