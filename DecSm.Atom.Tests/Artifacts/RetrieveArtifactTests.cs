﻿namespace DecSm.Atom.Tests.Artifacts;

[TestFixture]
public class RetrieveArtifactTests
{
    [Test]
    public async Task DownloadArtifact_Returns_Valid_Target()
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
            .CallTo(() => artifactProvider.RetrieveArtifacts(A<string[]>.That.IsSameSequenceAs(artifactNames), A<string>.Ignored))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection()
            .AddSingleton(artifactProvider)
            .BuildServiceProvider();

        // Act
        var instance = A.Fake<IRetrieveArtifact>(options => options.CallsBaseMethods());

        A
            .CallTo(() => instance.Services)
            .Returns(services);

        A
            .CallTo(() => instance.AtomArtifacts)
            .Returns([(string)artifactNames[0], (string)artifactNames[1]]);

        var target = instance.RetrieveArtifact(new());

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
            .CallTo(() => artifactProvider.RetrieveArtifacts(A<string[]>.That.IsSameSequenceAs(artifactNames), A<string>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}