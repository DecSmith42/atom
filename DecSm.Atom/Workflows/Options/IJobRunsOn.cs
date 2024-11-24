namespace DecSm.Atom.Workflows.Options;

public interface IJobRunsOn : IBuildDefinition
{
    [ParamDefinition("job-runs-on", "Runner/Agent to use for a job")]
    string JobRunsOn => GetParam(() => JobRunsOn)!;

    public const string WindowsLatestTag = "windows-latest";

    public const string UbuntuLatestTag = "ubuntu-latest";

    public const string MacOsLatestTag = "macos-latest";
}
