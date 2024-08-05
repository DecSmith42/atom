namespace DecSm.Atom.Tests;

[TestFixture]
internal sealed class SemVerBuildNumberExtractionTests
{
    [TestCase("alpha.beta.1", 1)]
    [TestCase("alpha.1", 1)]
    [TestCase("alpha3.valid", 3)]
    [TestCase("alpha.4valid", 4)]
    [TestCase("rc.1", 1)]
    [TestCase("alpha.1227", 1227)]
    [TestCase("7A.is.legal", 7)]
    [TestCase("SNAPSHOT-123", 123)]
    [TestCase("---RC-SNAPSHOT.12--N-A", 12)]
    [TestCase("prerelease", 0)]
    [TestCase("alpha", 0)]
    [TestCase("beta", 0)]
    [TestCase("alpha.beta", 0)]
    [TestCase("alpha-a.b-c-somethinglong", 0)]
    [TestCase("beta", 0)]
    [TestCase("DEV-SNAPSHOT", 0)]
    [TestCase("alpha", 0)]
    [TestCase("alpha3.4valid", 0)]
    [TestCase("3alpha.4valid.1", 0)]
    [TestCase("---RC-SNAPSHOT.12.9.1--.12", 0)]
    [TestCase("---R-S.12.9.1--.12", 0)]
    [TestCase("---RC-SNAPSHOT.12.9.1--.12", 0)]
    public void DefaultBuildNumberExtractionStrategy_ExtractsBuildNumber(string preRelease, int expected)
    {
        var actual = SemVer.ExtractBuildNumber(preRelease);

        actual.ShouldBe(expected);
    }
}