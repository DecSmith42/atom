namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[TestFixture]
public class WorkflowTests
{
    [Test]
    public void MinimalBuild_GeneratesNoWorkflows()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var build = CreateTestHost<MinimalBuild>(fileSystem: fileSystem, commandLineArgs: new(true, [new GenArg()]));

        // Act
        build.Run();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(@"C:\Atom\_atom\.github\workflows")
            .Exists
            .ShouldBeFalse();
    }

    [Test]
    public async Task SimpleBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var build = CreateTestHost<SimpleBuild>(fileSystem: fileSystem, commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(@"C:\Atom\_atom\.github\workflows")
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync(@"C:\Atom\_atom\.github\workflows\simple-build.yml");
        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task DependentBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var build = CreateTestHost<DependentBuild>(fileSystem: fileSystem, commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(@"C:\Atom\_atom\.github\workflows")
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync(@"C:\Atom\_atom\.github\workflows\dependent-build.yml");
        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }
}
