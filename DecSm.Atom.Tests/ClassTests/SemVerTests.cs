namespace DecSm.Atom.Tests.ClassTests;

[TestFixture]
internal sealed class SemVerTests
{
    [TestCase("1.0.0-alpha", true)]
    [TestCase("1.0.0", false)]
    public void IsPreRelease_ShouldReturnExpectedResult(string versionString, bool expectedResult)
    {
        var semVer = SemVer.Parse(versionString);

        semVer.IsPreRelease.ShouldBe(expectedResult);
    }

    [TestCase("1.0.0", "2.0.0", "1.5.0", true)]
    [TestCase("2.0.0", "1.0.0", "1.5.0", true)]
    [TestCase("1.0.0", "2.0.0", "2.0.0", false)]
    [TestCase("2.0.0", "1.0.0", "2.0.0", false)]
    [TestCase("1.0.0", "1.0.0", "1.0.0", true)]
    [TestCase("1.0.0", "1.0.0", "2.0.0", false)]
    public void IsBetween_ShouldReturnExpectedResult(
        string firstBound,
        string secondBound,
        string version,
        bool expectedResult)
    {
        // Arrange
        var first = SemVer.Parse(firstBound);
        var second = SemVer.Parse(secondBound);
        var semVer = SemVer.Parse(version);

        // Act
        var result = semVer.IsBetween(first, second);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [TestCase(1, 0, 0, 0, false, 1, 0, 0)]
    [TestCase(1, 2, 3, 0, false, 1, 2, 3)]
    [TestCase(1, 2, 3, 4, false, 1, 2, 3)]
    [TestCase(1, 2, 3, 0, true, 1, 2, 3)]
    public void FromSystemVersion_ShouldReturnExpectedSemVer(
        int major,
        int minor,
        int build,
        int revision,
        bool throwIfContainsRevision,
        int expectedMajor,
        int expectedMinor,
        int expectedPatch)
    {
        // Arrange
        var version = new Version(major, minor, build, revision);

        // Act
        var result = SemVer.FromSystemVersion(version, throwIfContainsRevision);

        // Assert
        result.Major.ShouldBe(expectedMajor);
        result.Minor.ShouldBe(expectedMinor);
        result.Patch.ShouldBe(expectedPatch);
    }

    [TestCase(1, 2, 3, 4, true)]
    public void FromSystemVersion_WithRevision_ShouldThrowArgumentException(
        int major,
        int minor,
        int build,
        int revision,
        bool throwIfContainsRevision)
    {
        // Arrange
        var version = new Version(major, minor, build, revision);

        // Act / Assert
        Should.Throw<ArgumentException>(() => SemVer.FromSystemVersion(version, throwIfContainsRevision));
    }

    [TestCase("1.0.0", 1, 0, 0)]
    [TestCase("2.1.0", 2, 1, 0)]
    [TestCase("3.2.1", 3, 2, 1)]
    [TestCase("1.0.0-alpha", 1, 0, 0, "alpha", null)]
    [TestCase("1.0.0+build", 1, 0, 0, null, "build")]
    [TestCase("1.0.0-alpha+build", 1, 0, 0, "alpha", "build")]
    public void Parse_ValidVersionString_ShouldReturnSemVer(
        string versionString,
        int expectedMajor,
        int expectedMinor,
        int expectedPatch,
        string? expectedPreRelease = null,
        string? expectedMetadata = null)
    {
        var semVer = SemVer.Parse(versionString);

        semVer.ShouldSatisfyAllConditions(() => semVer.Major.ShouldBe(expectedMajor),
            () => semVer.Minor.ShouldBe(expectedMinor),
            () => semVer.Patch.ShouldBe(expectedPatch),
            () => semVer.PreRelease.ShouldBe(expectedPreRelease),
            () => semVer.Metadata.ShouldBe(expectedMetadata));
    }

    [TestCase("1.0")]
    [TestCase("1.0.0.0")]
    [TestCase("1..0")]
    [TestCase("1.0.0-")]
    [TestCase("1.0.0+")]
    [TestCase("-1.0.0")]
    [TestCase("1.-1.0")]
    [TestCase("1.1.-1")]
    [TestCase("a.0.0")]
    [TestCase("1.a.0")]
    [TestCase("1.1.a")]
    public void Parse_InvalidVersionString_ShouldThrowArgumentException(string versionString) =>
        Should.Throw<ArgumentException>(() => SemVer.Parse(versionString));

    [TestCase("1.0.0", 1, 0, 0)]
    [TestCase("2.1.0", 2, 1, 0)]
    [TestCase("3.2.1", 3, 2, 1)]
    [TestCase("1.0.0-alpha", 1, 0, 0, "alpha", null)]
    [TestCase("1.0.0+build", 1, 0, 0, null, "build")]
    [TestCase("1.0.0-alpha+build", 1, 0, 0, "alpha", "build")]
    public void Parse_ValidVersionSpan_ShouldReturnSemVer(
        string versionString,
        int expectedMajor,
        int expectedMinor,
        int expectedPatch,
        string? expectedPreRelease = null,
        string? expectedMetadata = null)
    {
        var semVer = SemVer.Parse(versionString.AsSpan(), null);

        semVer.ShouldSatisfyAllConditions(() => semVer.Major.ShouldBe(expectedMajor),
            () => semVer.Minor.ShouldBe(expectedMinor),
            () => semVer.Patch.ShouldBe(expectedPatch),
            () => semVer.PreRelease.ShouldBe(expectedPreRelease),
            () => semVer.Metadata.ShouldBe(expectedMetadata));
    }

    [TestCase("1.0")]
    [TestCase("1.0.0.0")]
    [TestCase("1..0")]
    [TestCase("1.0.0-")]
    [TestCase("1.0.0+")]
    public void Parse_InvalidVersionSpan_ShouldThrowArgumentException(string versionString) =>
        Should.Throw<ArgumentException>(() => SemVer.Parse(versionString.AsSpan(), null));

    [TestCase("1.0.0", true)]
    [TestCase("1.0", false)]
    public void TryParse_ValidVersionString_ShouldReturnTrue(string versionString, bool expectedResult) =>
        SemVer
            .TryParse(versionString, null, out _)
            .ShouldBe(expectedResult);

    [TestCase("1.0.0", "1.0.0", 0)]
    [TestCase("1.0.0", "2.0.0", -1)]
    [TestCase("2.0.0", "1.0.0", 1)]
    [TestCase("1.0.0-alpha", "1.0.0-beta", -1)]
    [TestCase("1.0.0-beta", "1.0.0-alpha", 1)]
    public void CompareTo_ShouldReturnExpectedResult(string version1, string version2, int expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        semVer1
            .CompareTo(semVer2)
            .ShouldBe(expectedResult);
    }

    [TestCase("2.0.0", "1.0.0", true)]
    [TestCase("1.0.0", "2.0.0", false)]
    public void OperatorGreaterThan_ShouldReturnExpectedResult(string version1, string version2, bool expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        (semVer1 > semVer2).ShouldBe(expectedResult);
    }

    [TestCase("1.0.0", "2.0.0", true)]
    [TestCase("2.0.0", "1.0.0", false)]
    public void OperatorLessThan_ShouldReturnExpectedResult(string version1, string version2, bool expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        (semVer1 < semVer2).ShouldBe(expectedResult);
    }

    [TestCase("1.0.0", "1.0.0", true)]
    [TestCase("2.0.0", "1.0.0", true)]
    [TestCase("1.0.0", "2.0.0", false)]
    public void OperatorGreaterThanOrEqual_ShouldReturnExpectedResult(
        string version1,
        string version2,
        bool expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        (semVer1 >= semVer2).ShouldBe(expectedResult);
    }

    [TestCase("1.0.0", "1.0.0", true)]
    [TestCase("1.0.0", "2.0.0", true)]
    [TestCase("2.0.0", "1.0.0", false)]
    public void OperatorLessThanOrEqual_ShouldReturnExpectedResult(
        string version1,
        string version2,
        bool expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        (semVer1 <= semVer2).ShouldBe(expectedResult);
    }

    [TestCase("1.0.0", "1.0.0")]
    [TestCase("1.0.0-alpha", "1.0.0-alpha")]
    [TestCase("1.0.0+build", "1.0.0+build")]
    [TestCase("1.0.0-alpha+build", "1.0.0-alpha+build")]
    public void ToString_ShouldReturnCorrectFormat(string versionString, string expectedString)
    {
        var semVer = SemVer.Parse(versionString);

        semVer
            .ToString()
            .ShouldBe(expectedString);
    }

    [TestCase("1.0.0", 1, 0, 0)]
    [TestCase("1.0.0-alpha", 1, 0, 0, "alpha", null)]
    [TestCase("1.0.0+build", 1, 0, 0, null, "build")]
    [TestCase("1.0.0-alpha+build", 1, 0, 0, "alpha", "build")]
    public void ImplicitConversionFromString_ShouldReturnSemVer(
        string versionString,
        int expectedMajor,
        int expectedMinor,
        int expectedPatch,
        string? expectedPreRelease = null,
        string? expectedMetadata = null)
    {
        SemVer semVer = versionString;

        semVer.ShouldSatisfyAllConditions(() => semVer.Major.ShouldBe(expectedMajor),
            () => semVer.Minor.ShouldBe(expectedMinor),
            () => semVer.Patch.ShouldBe(expectedPatch),
            () => semVer.PreRelease.ShouldBe(expectedPreRelease),
            () => semVer.Metadata.ShouldBe(expectedMetadata));
    }

    [TestCase("1.0.0", "1.0.0")]
    [TestCase("1.0.0-alpha", "1.0.0-alpha")]
    [TestCase("1.0.0+build", "1.0.0+build")]
    [TestCase("1.0.0-alpha+build", "1.0.0-alpha+build")]
    public void ImplicitConversionToString_ShouldReturnString(string versionString, string expectedString)
    {
        SemVer semVer = versionString;

        string actualString = semVer;

        actualString.ShouldBe(expectedString);
    }

    [TestCase("1.0.0", "1.0.0")]
    [TestCase("1.0.0-alpha", "1.0.0-alpha")]
    [TestCase("1.0.0+build", "1.0.0+build")]
    [TestCase("1.0.0-alpha+build", "1.0.0-alpha+build")]
    public void Serialize_Deserialize_Json(string versionString, string expectedString)
    {
        var semVer = SemVer.Parse(versionString);

        var json = JsonSerializer.Serialize(semVer, JsonSerializerOptions.Default);
        var actual = JsonSerializer.Deserialize<SemVer>(json, JsonSerializerOptions.Default)!;

        actual
            .ToString()
            .ShouldBe(expectedString);
    }
}
