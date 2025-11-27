namespace DecSm.Atom.Module.DevopsWorkflows;

[PublicAPI]
public static class Extensions
{
    public static WorkflowTargetDefinition WithDevopsPoolMatrix(
        this WorkflowTargetDefinition workflowTargetDefinition,
        string[] labels) =>
        workflowTargetDefinition
            .WithMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
            {
                Values = labels,
            })
            .WithOptions(DevopsPool.SetByMatrix);
}
