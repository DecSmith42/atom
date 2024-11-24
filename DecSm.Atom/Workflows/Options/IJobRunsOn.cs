namespace DecSm.Atom.Workflows.Options;

public interface IJobRunsOn : IBuildDefinition
{
    [ParamDefinition("job-runs-on", "Runner/Agent to use for a job")]
    string JobRunsOn => GetParam(() => JobRunsOn)!;
}
