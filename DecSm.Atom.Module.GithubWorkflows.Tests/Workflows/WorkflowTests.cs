﻿namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[TestFixture]
public class WorkflowTests
{
    private static string WorkflowDir =>
        Environment.OSVersion.Platform is PlatformID.Win32NT
            ? @"C:\Atom\_atom\.github\workflows\"
            : "/Atom/_atom/.github/workflows/";

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

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}simple-workflow.yml");

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

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}dependent-workflow.yml");

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

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}artifact-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task CustomArtifactBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var console = new TestConsole();

        var build = CreateTestHost<CustomArtifactBuild>(console,
            fileSystem,
            new(true, [new GenArg()]),
            configure: builder => builder.Services.AddSingleton<IArtifactProvider, TestArtifactProvider>());

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue(console.Output);

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}custom-artifact-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task ManualInputBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var build = CreateTestHost<ManualInputBuild>(fileSystem: fileSystem, commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}manual-input-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task SetupDotnetBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var build = CreateTestHost<SetupDotnetBuild>(fileSystem: fileSystem, commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}setup-dotnet.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }

    [Test]
    public async Task ReleaseTriggerBuild_GeneratesWorkflow()
    {
        // Arrange
        var fileSystem = FileSystemUtils.DefaultMockFileSystem;
        var build = CreateTestHost<ReleaseTriggerBuild>(fileSystem: fileSystem, commandLineArgs: new(true, [new GenArg()]));

        // Act
        await build.RunAsync();

        // Assert
        fileSystem
            .DirectoryInfo
            .New(WorkflowDir)
            .Exists
            .ShouldBeTrue();

        var workflow = await fileSystem.File.ReadAllTextAsync($"{WorkflowDir}releasetrigger-workflow.yml");

        await Verify(workflow);
        await TestContext.Out.WriteAsync(workflow);
    }
}
