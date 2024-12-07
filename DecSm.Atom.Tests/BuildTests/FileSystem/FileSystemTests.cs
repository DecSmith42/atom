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
        #if Win32NT
        atomRootDirectory
            .ToString()
            .ShouldBe(@"C:\Atom\_atom");
        #else
        atomRootDirectory
            .ToString()
            .ShouldBe(@"/Atom/_atom");
        #endif
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
                        #if Win32NT
                        .CreateAbsolutePath(@"C:\CustomAtomRoot")
                        #else
                        .CreateAbsolutePath(@"/CustomAtomRoot")
                        #endif
                    : null,
                Priority = 0,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomRootDirectory;

        // Assert
        #if Win32NT
        atomRootDirectory
            .ToString()
            .ShouldBe(@"C:\CustomAtomRoot");
        #else
        atomRootDirectory
            .ToString()
            .ShouldBe(@"/CustomAtomRoot");
        #endif
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
        #if Win32NT
        atomRootDirectory
            .ToString()
            .ShouldBe(@"C:\Atom\_atom\atom-publish");
        #else
        atomRootDirectory
            .ToString()
            .ShouldBe(@"/Atom/_atom/atom-publish");
        #endif
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
                        #if Win32NT
                        .CreateAbsolutePath(@"C:\CustomAtomArtifacts")
                        #else
                        .CreateAbsolutePath(@"/CustomAtomArtifacts")
                        #endif
                    : null,
                Priority = 0,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomArtifactsDirectory;

        // Assert
        #if Win32NT
        atomRootDirectory
            .ToString()
            .ShouldBe(@"C:\CustomAtomArtifacts");
        #else
        atomRootDirectory
            .ToString()
            .ShouldBe(@"/CustomAtomArtifacts");
        #endif
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
        #if Win32NT
        atomRootDirectory
            .ToString()
            .ShouldBe(@"C:\Atom\_atom\atom-publish");
        #else
        atomRootDirectory
            .ToString()
            .ShouldBe(@"/Atom/_atom/atom-publish");
        #endif
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
                        #if Win32NT
                        .CreateAbsolutePath(@"C:\CustomAtomPublish")
                        #else
                        .CreateAbsolutePath(@"/CustomAtomPublish")
                        #endif
                    : null,
                Priority = 0,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomPublishDirectory;

        // Assert
        #if Win32NT
        atomRootDirectory
            .ToString()
            .ShouldBe(@"C:\CustomAtomPublish");
        #else
        atomRootDirectory
            .ToString()
            .ShouldBe(@"/CustomAtomPublish");
        #endif
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
        #if Win32NT
        atomRootDirectory
            .ToString()
            .ShouldBe(@"C:\temp");
        #else
        atomRootDirectory
            .ToString()
            .ShouldBe(@"/temp");
        #endif
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
                        #if Win32NT
                        .CreateAbsolutePath(@"C:\CustomAtomTemp")
                        #else
                        .CreateAbsolutePath(@"/CustomAtomTemp")
                        #endif
                    : null,
                Priority = 0,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomTempDirectory;

        // Assert
        #if Win32NT
        atomRootDirectory
            .ToString()
            .ShouldBe(@"C:\CustomAtomTemp");
        #else
        atomRootDirectory
            .ToString()
            .ShouldBe(@"/CustomAtomTemp");
        #endif
    }
}
