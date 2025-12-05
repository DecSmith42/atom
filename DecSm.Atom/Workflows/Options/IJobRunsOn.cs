namespace DecSm.Atom.Workflows.Options;

/// <summary>
///     Defines the execution environment (runner or agent) for a workflow job.
/// </summary>
/// <remarks>
///     This interface provides common constants for operating system tags and a parameter for specifying the runner.
///     It is typically used in build matrices to run jobs across multiple environments.
/// </remarks>
[PublicAPI]
public interface IJobRunsOn : IBuildDefinition, IBuildAccessor
{
    /// <summary>
    ///     Represents the tag for the latest available Windows runner.
    /// </summary>
    const string WindowsLatestTag = "windows-latest";

    /// <summary>
    ///     Represents the tag for the latest available Ubuntu runner.
    /// </summary>
    const string UbuntuLatestTag = "ubuntu-latest";

    /// <summary>
    ///     Represents the tag for the latest available macOS runner.
    /// </summary>
    const string MacOsLatestTag = "macos-latest";

    /// <summary>
    ///     Gets the runner or agent tag for the job, sourced from the "job-runs-on" parameter.
    /// </summary>
    [ParamDefinition("job-runs-on", "The runner or agent to use for a job (e.g., 'windows-latest').")]
    string JobRunsOn => GetParam(() => JobRunsOn)!;
}
