namespace DecSm.Atom.Workflows.Options;

/// <summary>
///     Defines the execution environment (runner or agent) for a workflow job.
///     Implementations of this interface specify where a job will run.
/// </summary>
/// <remarks>
///     This interface is typically implemented by specific workflow definitions (e.g., for GitHub Actions or Azure
///     DevOps).
///     It provides common constants for operating system tags like <see cref="WindowsLatestTag" />,
///     <see cref="UbuntuLatestTag" />, and
///     <see cref="MacOsLatestTag" />.
///     The actual runner is determined by the <see cref="JobRunsOn" /> property, which is often configured externally.
///     It can be used in build matrices to run jobs across multiple environments.
/// </remarks>
[PublicAPI]
public interface IJobRunsOn : IBuildDefinition, IBuildAccessor
{
    /// <summary>
    ///     Tag for the latest Windows runner.
    /// </summary>
    const string WindowsLatestTag = "windows-latest";

    /// <summary>
    ///     Tag for the latest Ubuntu runner.
    /// </summary>
    const string UbuntuLatestTag = "ubuntu-latest";

    /// <summary>
    ///     Tag for the latest macOS runner.
    /// </summary>
    const string MacOsLatestTag = "macos-latest";

    /// <summary>
    ///     Gets the runner or agent tag for the job.
    /// </summary>
    /// <remarks>
    ///     This value is typically sourced from a parameter named "job-runs-on".
    ///     It can be one of the predefined constants (e.g., <see cref="WindowsLatestTag" />) or a custom runner/agent label.
    /// </remarks>
    [ParamDefinition("job-runs-on", "Runner/Agent to use for a job")]
    string JobRunsOn => GetParam(() => JobRunsOn)!;
}
