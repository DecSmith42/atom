namespace DecSm.Atom.Build.Model;

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