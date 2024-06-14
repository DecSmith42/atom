namespace DecSm.Atom.Build.Model;

public sealed record TargetState
{
    public TargetRunState Status { get; set; } = TargetRunState.Uninitialized;

    public TimeSpan? RunDuration { get; set; }
}