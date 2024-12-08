namespace DecSm.Atom.Tests.Utils;

public class TestBuildVersionProvider : IBuildVersionProvider
{
    public SemVer Version { get; set; } = SemVer.Parse("1.0.0");
}
