namespace DecSm.Atom.Tests;

[TestFixture]
internal sealed class BuildTimeStampTests
{
    private sealed class TimeProviderStub(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            utcNow;
    }

    [Test]
    public void Create_FromDateTimeOffset_ShouldReturnExpectedBuildStamp()
    {
        // Arrange
        var time = new DateTimeOffset(2023, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var expectedValue = (int)((time - DateTimeOffset.UnixEpoch).TotalSeconds / 10.0);

        // Act
        var result = BuildTimeStamp.Create(time);

        // Assert
        result.ShouldBe(new(expectedValue));
    }

    [Test]
    public void Create_FromTimeProvider_ShouldReturnExpectedBuildStamp()
    {
        // Arrange
        var timeProvider = new TimeProviderStub(new(2023, 10, 1, 0, 0, 0, TimeSpan.Zero));
        var expectedValue = (int)((timeProvider.GetUtcNow() - DateTimeOffset.UnixEpoch).TotalSeconds / 10.0);

        // Act
        var result = BuildTimeStamp.Create(timeProvider);

        // Assert
        result.ShouldBe(new(expectedValue));
    }

    [Test]
    public void GetUtc_ShouldReturnExpectedDateTimeOffset()
    {
        // Arrange
        var buildStamp = new BuildTimeStamp(123456);
        var expectedDateTime = DateTimeOffset.UnixEpoch.AddSeconds(123456 * 10);

        // Act
        var result = buildStamp.GetUtc();

        // Assert
        result.ShouldBe(expectedDateTime);
    }

    [Test]
    public void ToString_ShouldReturnExpectedString()
    {
        // Arrange
        var buildStamp = new BuildTimeStamp(123456);

        // Act
        var result = buildStamp.ToString();

        // Assert
        result.ShouldBe("123456");
    }

    [Test]
    public void ToDateTimeString_ShouldReturnExpectedString()
    {
        // Arrange
        var buildStamp = new BuildTimeStamp(123456);

        var expectedString = buildStamp
            .GetUtc()
            .ToString();

        // Act
        var result = buildStamp.ToDateTimeString();

        // Assert
        result.ShouldBe(expectedString);
    }

    [Test]
    public void CompareTo_ShouldReturnExpectedResult()
    {
        // Arrange
        var buildStamp1 = new BuildTimeStamp(123456);
        var buildStamp2 = new BuildTimeStamp(654321);

        // Act & Assert
        buildStamp1
            .CompareTo(buildStamp2)
            .ShouldBeLessThan(0);

        buildStamp2
            .CompareTo(buildStamp1)
            .ShouldBeGreaterThan(0);

        buildStamp1
            .CompareTo(buildStamp1)
            .ShouldBe(0);
    }

    [Test]
    public void Equals_ShouldReturnExpectedResult()
    {
        // Arrange
        var buildStamp1 = new BuildTimeStamp(123456);
        var buildStamp2 = new BuildTimeStamp(123456);
        var buildStamp3 = new BuildTimeStamp(654321);

        // Act & Assert
        buildStamp1
            .Equals(buildStamp2)
            .ShouldBeTrue();

        buildStamp1
            .Equals(buildStamp3)
            .ShouldBeFalse();
    }

    [Test]
    public void ImplicitConversion_ShouldReturnExpectedResult()
    {
        // Arrange
        var buildStamp = new BuildTimeStamp(123456);
        var expectedValue = 123456;

        // Act & Assert
        ((int)buildStamp).ShouldBe(expectedValue);
        ((BuildTimeStamp)expectedValue).ShouldBe(buildStamp);
    }

    [Test]
    public void ComparisonOperators_ShouldReturnExpectedResult()
    {
        // Arrange
        var buildStamp1 = new BuildTimeStamp(123456);
        var buildStamp2 = new BuildTimeStamp(654321);

        // Act & Assert
        (buildStamp1 < buildStamp2).ShouldBeTrue();
        (buildStamp1 <= buildStamp2).ShouldBeTrue();
        (buildStamp1 > buildStamp2).ShouldBeFalse();
        (buildStamp1 >= buildStamp2).ShouldBeFalse();
        (buildStamp1 == new BuildTimeStamp(123456)).ShouldBeTrue();
        (buildStamp1 != buildStamp2).ShouldBeTrue();
    }
}