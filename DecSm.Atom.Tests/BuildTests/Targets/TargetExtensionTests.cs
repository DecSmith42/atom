namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class TargetExtensionBuild : BuildDefinition, IBaseExtensionTarget, IExtendedExtensionTarget
{
    public bool BaseTargetExecuted { get; set; }

    public bool ExtendedTargetExecuted { get; set; }
}

[TargetDefinition]
public partial interface IBaseExtensionTarget
{
    bool BaseTargetExecuted { get; set; }

    Target BaseTarget =>
        d => d.Executes(() =>
        {
            BaseTargetExecuted = true;

            return Task.CompletedTask;
        });
}

[TargetDefinition]
public partial interface IExtendedExtensionTarget
{
    bool ExtendedTargetExecuted { get; set; }

    Target ExtendedTarget =>
        d => d
            .Extends<IBaseExtensionTarget>(definition => definition.BaseTarget)
            .Executes(() =>
            {
                ExtendedTargetExecuted = true;

                return Task.CompletedTask;
            });
}

[TestFixture]
public class TargetExtensionTests
{
    [Test]
    public void Target_Extension_ExecutesBaseTarget()
    {
        // Arrange
        var host = CreateTestHost<TargetExtensionBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IExtendedExtensionTarget.ExtendedTarget))]));

        var build = (TargetExtensionBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.BaseTargetExecuted.ShouldBeTrue();
        build.ExtendedTargetExecuted.ShouldBeTrue();
    }
}
