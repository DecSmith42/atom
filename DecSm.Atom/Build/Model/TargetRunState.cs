namespace DecSm.Atom.Build.Model;

/// <summary>
///     Represents the state of a target during a build run.
/// </summary>
[PublicAPI]
public enum TargetRunState
{
    /// <summary>
    ///     Default value, indicates that the target has not been initialized.
    /// </summary>
    Uninitialized,

    /// <summary>
    ///     The target is scheduled to run.
    /// </summary>
    PendingRun,

    /// <summary>
    ///     The target is running.
    /// </summary>
    Running,

    /// <summary>
    ///     The target has completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    ///     The target has failed.
    /// </summary>
    Failed,

    /// <summary>
    ///     The target will not be run.
    /// </summary>
    NotRun,

    /// <summary>
    ///     The target was scheduled to run but was skipped.
    /// </summary>
    Skipped,
}
