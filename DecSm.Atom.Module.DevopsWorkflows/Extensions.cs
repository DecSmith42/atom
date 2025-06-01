namespace DecSm.Atom.Module.DevopsWorkflows;

[PublicAPI]
public static class Extensions
{
    public static WorkflowTargetDefinition WithDevopsRunnerMatrix(
        this WorkflowTargetDefinition workflowTargetDefinition,
        string[] labels) =>
        workflowTargetDefinition
            .WithAddedMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn), labels))
            .WithAddedOptions(DevopsPool.SetByMatrix);
}
