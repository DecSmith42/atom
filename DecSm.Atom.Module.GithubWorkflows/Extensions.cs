namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
public static class Extensions
{
    public static CommandDefinition WithGithubRunnerMatrix(this CommandDefinition commandDefinition, string[] labels) =>
        commandDefinition
            .WithAddedMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn), labels))
            .WithAddedOptions(GithubRunsOn.SetByMatrix);
}
