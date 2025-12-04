namespace DecSm.Atom.Tests.BuildTests.BuildInfo;

[MinimalBuildDefinition]
public sealed partial class BuildInfoBuild : IBuildInfoTarget
{
    public string? BuildNameResult { get; set; }

    public string? BuildIdResult { get; set; }

    public SemVer? BuildVersionResult { get; set; }

    public long BuildTimestampResult { get; set; }
}

public interface IBuildInfoTarget : IBuildInfo
{
    Target BuildInfoTarget =>
        t => t.Executes(() =>
        {
            ((BuildInfoBuild)this).BuildNameResult = BuildName;
            ((BuildInfoBuild)this).BuildIdResult = BuildId;
            ((BuildInfoBuild)this).BuildVersionResult = BuildVersion;
            ((BuildInfoBuild)this).BuildTimestampResult = BuildTimestamp;
        });
}
