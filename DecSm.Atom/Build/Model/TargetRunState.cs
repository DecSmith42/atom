namespace DecSm.Atom.Build.Model;

public enum TargetRunState
{
    Uninitialized,
    PendingRun,
    PendingSkip,
    Running,
    Succeeded,
    Failed,
    Skipped,
}