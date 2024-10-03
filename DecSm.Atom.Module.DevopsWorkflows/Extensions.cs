namespace DecSm.Atom.Module.DevopsWorkflows;

public static class Extensions
{
    public static CommandDefinition WithDevopsRunnerMatrix(this CommandDefinition commandDefinition, string[] labels) =>
        commandDefinition
            .WithAddedMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn), labels))
            .WithAddedOptions(DevopsPool.MatrixDefined);
}
