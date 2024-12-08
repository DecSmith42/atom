namespace DecSm.Atom.Tests.BuildTests.BuildModel;

[TestFixture]
public class BuildModelTests
{
    [Test]
    public void BuildModel_MinimalDefinition_IsEmpty()
    {
        // Arrange
        var host = CreateTestHost<MinimalAtomBuild>();

        // Act
        var buildModel = host.Services.GetRequiredService<Build.Model.BuildModel>();

        // Assert
        buildModel.ShouldSatisfyAllConditions(b => b.Targets.ShouldBeEmpty(),
            b => b.TargetStates.ShouldBeEmpty(),
            b => b.CurrentTarget.ShouldBeNull());
    }

    [Test]
    public async Task BuildModel_DefaultBuildDefinition_HasDefaultTargets()
    {
        // Arrange
        var host = CreateTestHost<DefaultAtomBuild>();

        // Act
        var buildModel = host.Services.GetRequiredService<Build.Model.BuildModel>();

        // Assert
        await Verify(buildModel);
    }
}
