﻿namespace DecSm.Atom.Tests.Artifacts;

[TestFixture]
public class StoreArtifactTests
{
    [Test]
    public async Task UploadArtifact_Returns_Valid_Target()
    {
        Assert.Ignore("Need to wait for FakeItEasy to update with Castle.Core 5.2.0 with support for default interface methods.");

        // Arrange
        object?[] artifactNames = ["artifact1", "artifact2"];

        var artifactProvider = A.Fake<IArtifactProvider>();

        A
            .CallTo(() => artifactProvider.RequiredParams)
            .Returns(new List<string>
            {
                "param1",
                "param2",
            });

        A
            .CallTo(() => artifactProvider.StoreArtifacts(A<string[]>.That.IsSameSequenceAs(artifactNames), A<string?>._))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection()
            .AddSingleton(artifactProvider)
            .BuildServiceProvider();

        var instance = A.Fake<IStoreArtifact>(options => options.CallsBaseMethods());

        A
            .CallTo(() => instance.Services)
            .Returns(services);

        A
            .CallTo(() => instance.AtomArtifacts)
            .Returns([(string)artifactNames[0], (string)artifactNames[1]]);

        // Act
        var target = instance.StoreArtifact(new());

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

        A
            .CallTo(() => artifactProvider.StoreArtifacts(A<string[]>.That.IsSameSequenceAs(artifactNames), A<string?>._))
            .MustHaveHappenedOnceExactly();
    }
}