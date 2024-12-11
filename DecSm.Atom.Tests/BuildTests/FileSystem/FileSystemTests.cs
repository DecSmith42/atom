namespace DecSm.Atom.Tests.BuildTests.FileSystem;

[TestFixture]
public sealed class FileSystemTests
{
    [Test]
    public void Minimal_BuildDefinition_WithDefaultLocation_Locates_AtomRootDirectory()
    {
        // Arrange
        var host = CreateTestHost<MinimalAtomBuild>();

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomRootDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\Atom\_atom"
                : "/Atom/_atom");
    }

    [Test]
    public void Minimal_BuildDefinition_WithCustomLocator_Locates_AtomRootDirectory()
    {
        // Arrange
        var host = CreateTestHost<MinimalAtomBuild>(configure: builder => builder.Services.AddSingleton<IPathProvider>(provider =>
            new PathProvider
            {
                Locator = (key, _) => key is AtomPaths.Root
                    ? provider
                        .GetRequiredService<IAtomFileSystem>()
                        .CreateAbsolutePath(Environment.OSVersion.Platform is PlatformID.Win32NT
                            ? @"C:\CustomAtomRoot"
                            : "/CustomAtomRoot")
                    : null,
                Priority = 0,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomRootDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\CustomAtomRoot"
                : "/CustomAtomRoot");
    }

    [Test]
    public void Minimal_BuildDefinition_WithDefaultLocation_Locates_AtomArtifactsDirectory()
    {
        // Arrange
        var host = CreateTestHost<MinimalAtomBuild>();

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomArtifactsDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\Atom\_atom\atom-publish"
                : "/Atom/_atom/atom-publish");
    }

    [Test]
    public void Minimal_BuildDefinition_WithCustomLocator_Locates_AtomArtifactsDirectory()
    {
        // Arrange
        var host = CreateTestHost<MinimalAtomBuild>(configure: builder => builder.Services.AddSingleton<IPathProvider>(provider =>
            new PathProvider
            {
                Locator = (key, _) => key is AtomPaths.Artifacts
                    ? provider
                        .GetRequiredService<IAtomFileSystem>()
                        .CreateAbsolutePath(Environment.OSVersion.Platform is PlatformID.Win32NT
                            ? @"C:\CustomAtomArtifacts"
                            : "/CustomAtomArtifacts")
                    : null,
                Priority = 0,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomArtifactsDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\CustomAtomArtifacts"
                : "/CustomAtomArtifacts");
    }

    [Test]
    public void Minimal_BuildDefinition_WithDefaultLocation_Locates_AtomPublishDirectory()
    {
        // Arrange
        var host = CreateTestHost<MinimalAtomBuild>();

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomPublishDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\Atom\_atom\atom-publish"
                : "/Atom/_atom/atom-publish");
    }

    [Test]
    public void Minimal_BuildDefinition_WithCustomLocator_Locates_AtomPublishDirectory()
    {
        // Arrange
        var host = CreateTestHost<MinimalAtomBuild>(configure: builder => builder.Services.AddSingleton<IPathProvider>(provider =>
            new PathProvider
            {
                Locator = (key, _) => key is AtomPaths.Publish
                    ? provider
                        .GetRequiredService<IAtomFileSystem>()
                        .CreateAbsolutePath(Environment.OSVersion.Platform is PlatformID.Win32NT
                            ? @"C:\CustomAtomPublish"
                            : "/CustomAtomPublish")
                    : null,
                Priority = 0,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomPublishDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\CustomAtomPublish"
                : "/CustomAtomPublish");
    }

    [Test]
    public void Minimal_BuildDefinition_WithDefaultLocation_Locates_AtomTempDirectory()
    {
        // Arrange
        var host = CreateTestHost<MinimalAtomBuild>();

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomTempDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\temp\"
                : "/temp");
    }

    [Test]
    public void Minimal_BuildDefinition_WithCustomLocator_Locates_AtomTempDirectory()
    {
        // Arrange
        var host = CreateTestHost<MinimalAtomBuild>(configure: builder => builder.Services.AddSingleton<IPathProvider>(provider =>
            new PathProvider
            {
                Locator = (key, _) => key is AtomPaths.Temp
                    ? provider
                        .GetRequiredService<IAtomFileSystem>()
                        .CreateAbsolutePath(Environment.OSVersion.Platform is PlatformID.Win32NT
                            ? @"C:\CustomAtomTemp"
                            : "/CustomAtomTemp")
                    : null,
                Priority = 0,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomTempDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\CustomAtomTemp"
                : "/CustomAtomTemp");
    }
}
