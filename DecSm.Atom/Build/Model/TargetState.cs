namespace DecSm.Atom.Build.Model;

[PublicAPI]
public sealed record TargetState(string Name)
{
    public TargetRunState Status { get; set; } = TargetRunState.Uninitialized;

    public TimeSpan? RunDuration { get; set; }
}
