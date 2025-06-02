namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public interface IJobRunsOn : IBuildDefinition, IBuildAccessor
{
    public const string WindowsLatestTag = "windows-latest";

    public const string UbuntuLatestTag = "ubuntu-latest";

    public const string MacOsLatestTag = "macos-latest";

    [ParamDefinition("job-runs-on", "Runner/Agent to use for a job")]
    string JobRunsOn => GetParam(() => JobRunsOn)!;
}
