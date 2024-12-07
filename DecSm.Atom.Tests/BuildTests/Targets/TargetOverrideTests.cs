namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class TargetOverrideBuild : BuildDefinition, IOverrideTarget
{
    public bool BaseTargetExecuted { get; set; }

    public bool OverrideTargetExecuted { get; set; }
}

[TargetDefinition]
public partial interface IBaseOverrideTarget
{
    bool BaseTargetExecuted { get; set; }

    Target Target =>
        d => d.Executes(() =>
        {
            BaseTargetExecuted = true;

            return Task.CompletedTask;
        });
}

[TargetDefinition]
public partial interface IOverrideTarget : IBaseOverrideTarget
{
    bool OverrideTargetExecuted { get; set; }

    new Target Target =>
        d => d.Executes(() =>
        {
            OverrideTargetExecuted = true;

            return Task.CompletedTask;
        });
}

[TestFixture]
public class TargetOverrideTests
{
    [Test]
    public void Target_Override_ExecutesOverrideTarget()
    {
        // Arrange
        var host = CreateTestHost<TargetOverrideBuild>(commandLineArgs: new(true, [new CommandArg(nameof(IBaseOverrideTarget.Target))]));

        var build = (TargetOverrideBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.BaseTargetExecuted.ShouldBeFalse();
        build.OverrideTargetExecuted.ShouldBeTrue();
    }
}
