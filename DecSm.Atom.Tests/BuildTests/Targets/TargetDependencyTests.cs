namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class TestTargetDependencyAtomBuild : BuildDefinition, ITestTarget1, ITestTarget2, ITestFailTarget1, ITestFailTarget2
{
    public bool Target1Executed { get; set; }

    public bool Target2Executed { get; set; }

    public bool TestFailTarget1Executed { get; set; }

    public bool TestFailTarget2Executed { get; set; }
}

[TargetDefinition]
public partial interface ITestTarget1
{
    bool Target1Executed { get; set; }

    Target TestTarget1 =>
        d => d.Executes(() =>
        {
            Target1Executed = true;

            return Task.CompletedTask;
        });
}

[TargetDefinition]
public partial interface ITestTarget2
{
    bool Target2Executed { get; set; }

    Target TestTarget2 =>
        d => d
            .DependsOn(TestTargetDependencyAtomBuild.Commands.TestTarget1)
            .Executes(() =>
            {
                Target2Executed = true;

                return Task.CompletedTask;
            });
}

[TargetDefinition]
public partial interface ITestFailTarget1
{
    bool TestFailTarget1Executed { get; set; }

    Target TestFailTarget1 =>
        d => d.Executes(() =>
        {
            TestFailTarget1Executed = true;

            throw new("TestFailTarget1 failed");
        });
}

[TargetDefinition]
public partial interface ITestFailTarget2
{
    bool TestFailTarget2Executed { get; set; }

    Target TestFailTarget2 =>
        d => d
            .DependsOn(TestTargetDependencyAtomBuild.Commands.TestFailTarget1)
            .Executes(() =>
            {
                TestFailTarget2Executed = true;

                return Task.CompletedTask;
            });
}

[TestFixture]
public class TargetDependencyTests
{
    [Test]
    public void TestDependencyTargetBuild_WithFirstTargetArg_ExecutesFirstTargetOnly()
    {
        // Arrange
        var host = CreateTestHost<TestTargetDependencyAtomBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(ITestTarget1.TestTarget1))]));

        var build = (TestTargetDependencyAtomBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.Target1Executed.ShouldBeTrue();
        build.Target2Executed.ShouldBeFalse();
    }

    [Test]
    public void TestDependencyTargetBuild_WithSecondTargetArg_ExecutesBothTargets()
    {
        // Arrange
        var host = CreateTestHost<TestTargetDependencyAtomBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(ITestTarget2.TestTarget2))]));

        var build = (TestTargetDependencyAtomBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.Target1Executed.ShouldBeTrue();
        build.Target2Executed.ShouldBeTrue();
    }

    [Test]
    public void TestDependencyTargetBuild_WithSecondTargetArg_AndFirstTargetFail_DoesNotExecuteSecondTarget()
    {
        // Arrange
        var host = CreateTestHost<TestTargetDependencyAtomBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(ITestFailTarget2.TestFailTarget2))]));

        var build = (TestTargetDependencyAtomBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.Target1Executed.ShouldBeFalse();
        build.Target2Executed.ShouldBeFalse();
        build.TestFailTarget1Executed.ShouldBeTrue();
        build.TestFailTarget2Executed.ShouldBeFalse();
    }
}
