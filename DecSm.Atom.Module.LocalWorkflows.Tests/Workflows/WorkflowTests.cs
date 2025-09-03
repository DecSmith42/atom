namespace DecSm.Atom.Module.LocalWorkflows.Tests.Workflows;

[TestFixture]
public class WorkflowTests
{
    private static string WorkflowDir =>
        Environment.OSVersion.Platform is PlatformID.Win32NT
            ? @"C:\Atom\_atom\.atom\workflows\"
            : "/Atom/_atom/.atom/workflows/";

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
            .New(WorkflowDir)
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
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}simple-workflow.ps1");

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
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}dependent-workflow.ps1");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task ArtifactBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var console = new TestConsole();
        var build = CreateTestHost<ArtifactBuild>(console, fileSystem, new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue(console.Output);

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}artifact-workflow.ps1");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task EnvironmentBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var build = CreateTestHost<EnvironmentBuild>(fileSystem: fileSystem, commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}environment-workflow.ps1");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }
}
