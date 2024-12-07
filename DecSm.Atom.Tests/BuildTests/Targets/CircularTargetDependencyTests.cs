namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class TestCircularTargetDependencyAtomBuild1 : BuildDefinition, ITestCircularTarget1, ITestCircularTarget2
{
    public bool CircularTarget1Executed { get; set; }

    public bool CircularTarget2Executed { get; set; }
}

[TargetDefinition]
public partial interface ITestCircularTarget1
{
    bool CircularTarget1Executed { get; set; }

    Target TestCircularTarget1 =>
        d => d
            .DependsOn(TestCircularTargetDependencyAtomBuild1.Commands.TestCircularTarget2)
            .Executes(() =>
            {
                CircularTarget1Executed = true;

                return Task.CompletedTask;
            });
}

[TargetDefinition]
public partial interface ITestCircularTarget2
{
    bool CircularTarget2Executed { get; set; }

    Target TestCircularTarget2 =>
        d => d
            .DependsOn(TestCircularTargetDependencyAtomBuild1.Commands.TestCircularTarget1)
            .Executes(() =>
            {
                CircularTarget2Executed = true;

                return Task.CompletedTask;
            });
}

[BuildDefinition]
public partial class TestCircularTargetDependencyAtomBuild2 : BuildDefinition,
    ITestCircularTarget3,
    ITestCircularTarget4,
    ITestCircularTarget5
{
    public bool CircularTarget3Executed { get; set; }

    public bool CircularTarget4Executed { get; set; }

    public bool CircularTarget5Executed { get; set; }
}

[TargetDefinition]
public partial interface ITestCircularTarget3
{
    bool CircularTarget3Executed { get; set; }

    Target TestCircularTarget3 =>
        d => d
            .DependsOn(TestCircularTargetDependencyAtomBuild2.Commands.TestCircularTarget4)
            .Executes(() =>
            {
                CircularTarget3Executed = true;

                return Task.CompletedTask;
            });
}

[TargetDefinition]
public partial interface ITestCircularTarget4
{
    bool CircularTarget4Executed { get; set; }

    Target TestCircularTarget4 =>
        d => d
            .DependsOn(TestCircularTargetDependencyAtomBuild2.Commands.TestCircularTarget5)
            .Executes(() =>
            {
                CircularTarget4Executed = true;

                return Task.CompletedTask;
            });
}

[TargetDefinition]
public partial interface ITestCircularTarget5
{
    bool CircularTarget5Executed { get; set; }

    Target TestCircularTarget5 =>
        d => d
            .DependsOn(TestCircularTargetDependencyAtomBuild2.Commands.TestCircularTarget3)
            .Executes(() =>
            {
                CircularTarget5Executed = true;

                return Task.CompletedTask;
            });
}

[TestFixture]
public class CircularTargetDependencyTests
{
    [Test]
    public void TestCircularDependency1TargetBuild_Throws_Exception()
    {
        // Arrange
        var host = CreateTestHost<TestCircularTargetDependencyAtomBuild1>();
        var buildDefinition = (TestCircularTargetDependencyAtomBuild1)host.Services.GetRequiredService<IBuildDefinition>();

        // Act / Assert
        Should.Throw<Exception>(() => host.RunAsync(), TimeSpan.FromSeconds(5));
        buildDefinition.CircularTarget1Executed.ShouldBeFalse();
        buildDefinition.CircularTarget2Executed.ShouldBeFalse();
    }

    [Test]
    public void TestCircularDependency2TargetBuild_Throws_Exception()
    {
        // Arrange
        var host = CreateTestHost<TestCircularTargetDependencyAtomBuild2>();
        var buildDefinition = (TestCircularTargetDependencyAtomBuild2)host.Services.GetRequiredService<IBuildDefinition>();

        // Act / Assert
        Should.Throw<Exception>(() => host.RunAsync(), TimeSpan.FromSeconds(5));
        buildDefinition.CircularTarget3Executed.ShouldBeFalse();
        buildDefinition.CircularTarget4Executed.ShouldBeFalse();
        buildDefinition.CircularTarget5Executed.ShouldBeFalse();
    }
}
