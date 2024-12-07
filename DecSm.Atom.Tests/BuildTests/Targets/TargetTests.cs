namespace DecSm.Atom.Tests.BuildTests.Targets;

[TestFixture]
public class TargetTests
{
    [Test]
    public void TestTargetBuild_WithTestTargetArg_Executes_TestTarget()
    {
        // Arrange
        var executed = false;

        var host = CreateTestHost<TestTargetAtomBuild>(commandLineArgs: new(true, [new CommandArg("TestTarget")]));

        ((ITestTarget)host.Services.GetRequiredService<IBuildDefinition>()).Execute = () =>
        {
            executed = true;

            return Task.CompletedTask;
        };

        // Act
        host.Run();

        // Assert
        executed.ShouldBeTrue();
    }

    [Test]
    public void TestTargetBuild_WithoutTestTargetArg_Skips_TestTarget()
    {
        // Arrange
        var executed = false;

        var host = CreateTestHost<TestTargetAtomBuild>();

        ((ITestTarget)host.Services.GetRequiredService<IBuildDefinition>()).Execute = () =>
        {
            executed = true;

            return Task.CompletedTask;
        };

        // Act
        host.Run();

        // Assert
        executed.ShouldBeFalse();
    }
}
