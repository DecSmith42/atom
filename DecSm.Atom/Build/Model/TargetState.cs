namespace DecSm.Atom.Build.Model;

/// <summary>
///     Represents the state of a target during a build run.
/// </summary>
/// <param name="Name">The name of the target.</param>
[PublicAPI]
public sealed record TargetState(string Name)
{
    /// <summary>
    ///     The status of the target.
    /// </summary>
    public TargetRunState Status { get; set; } = TargetRunState.Uninitialized;

    /// <summary>
    ///     The time spent executing the target.
    /// </summary>
    /// <remarks>
    ///     Will be null until the target has succeeded or failed.
    /// </remarks>
    public TimeSpan? RunDuration { get; set; }
}
