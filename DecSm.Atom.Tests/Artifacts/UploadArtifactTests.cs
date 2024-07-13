namespace DecSm.Atom.Tests.Artifacts;

[TestFixture]
public class UploadArtifactTests
{
    [Test]
    public async Task UploadArtifact_Returns_Valid_Target()
    {
        // Arrange
        var artifactNames = new[] { "artifact1", "artifact2" };

        var artifactProvider = new Mock<IArtifactProvider>();

        artifactProvider
            .Setup(x => x.RequiredParams)
            .Returns(new[] { "param1", "param2" });

        artifactProvider
            .Setup(x => x.UploadArtifacts(It.Is<string[]>(s => s.SequenceEqual(artifactNames))))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once);

        var services = new ServiceCollection()
            .AddSingleton(artifactProvider.Object)
            .BuildServiceProvider();

        var instance = new Mock<IUploadArtifact>
        {
            CallBase = true,
        };

        instance
            .Setup(x => x.Services)
            .Returns(services);

        instance
            .Setup(x => x.AtomArtifacts)
            .Returns($"{artifactNames[0]};{artifactNames[1]}");

        // Act
        var target = instance.Object.UploadArtifact(new());

        // Assert
        target.ShouldSatisfyAllConditions(x => x.ShouldNotBeNull(),
            x => x.ConsumedVariables.ShouldBeEquivalentTo(new List<ConsumedVariable>
            {
                new(nameof(ISetup.Setup), nameof(ISetup.AtomBuildId)),
            }),
            x => x.RequiredParams.ShouldBeEquivalentTo(new List<string>
            {
                nameof(IArtifactHelper.AtomArtifacts),
                "param1",
                "param2",
            }),
            x => x.Tasks.ShouldNotBeEmpty());

        await target
            .Tasks[0]();

        artifactProvider.Verify();
    }
}