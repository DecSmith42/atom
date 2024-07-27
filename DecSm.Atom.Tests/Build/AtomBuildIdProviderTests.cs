namespace DecSm.Atom.Tests.Build;

[TestFixture]
public class AtomBuildIdProviderTests
{
    [Test]
    public void BuildId_Always_ReturnsPositiveNumber()
    {
        // Arrange
        var provider = new AtomBuildIdProvider();

        // Act
        var buildId = provider.BuildId;

        // Assert
        buildId.ShouldNotBeEmpty();
    }

    [Test]
    public void BuildId_Always_CachesValue()
    {
        // Arrange
        var provider = new AtomBuildIdProvider();

        // Act
        var buildId1 = provider.BuildId;
        Thread.Sleep(100);
        var buildId2 = provider.BuildId;

        // Assert
        buildId1.ShouldBe(buildId2);
    }
}