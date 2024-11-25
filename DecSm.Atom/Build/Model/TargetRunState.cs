namespace DecSm.Atom.Build.Model;

[PublicAPI]
public enum TargetRunState
{
    Uninitialized,
    PendingRun,
    Running,
    Succeeded,
    Failed,
    NotRun,
    Skipped,
}
