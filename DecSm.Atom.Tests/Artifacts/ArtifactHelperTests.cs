namespace DecSm.Atom.Tests.Artifacts;

[TestFixture]
public class ArtifactHelperTests
{
    [Test]
    public void AtomArtifactNames_Should_Return_Split_AtomArtifacts()
    {
        // Arrange
        const string input = "artifact1;artifact2;artifact3";
        var expected = new[] { "artifact1", "artifact2", "artifact3" };

        var instance = new Mock<IArtifactHelper>
        {
            CallBase = true,
        };

        instance
            .SetupGet(x => x.AtomArtifacts)
            .Returns(input);

        // Act
        var result = instance.Object.AtomArtifactNames;

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }

    [Test]
    public void AtomArtifactNames_Should_Trim_Entries()
    {
        // Arrange
        const string input = "  artifact1;artifact2;  artifact3  ";
        var expected = new[] { "artifact1", "artifact2", "artifact3" };

        var instance = new Mock<IArtifactHelper>
        {
            CallBase = true,
        };

        instance
            .SetupGet(x => x.AtomArtifacts)
            .Returns(input);

        // Act
        var result = instance.Object.AtomArtifactNames;

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }

    [Test]
    public void AtomArtifactNames_Should_Remove_Empty_Entries()
    {
        // Arrange
        const string input = "artifact1;;artifact2;;artifact3";
        var expected = new[] { "artifact1", "artifact2", "artifact3" };

        var instance = new Mock<IArtifactHelper>
        {
            CallBase = true,
        };

        instance
            .SetupGet(x => x.AtomArtifacts)
            .Returns(input);

        // Act
        var result = instance.Object.AtomArtifactNames;

        // Assert
        result.ShouldBeEquivalentTo(expected);
    }
}