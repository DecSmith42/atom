namespace DecSm.Atom.Extensions.GithubWorkflows;

public static class Extensions
{
    public static CommandDefinition WithGithubRunnerMatrix(this CommandDefinition commandDefinition, string[] labels) =>
        commandDefinition
            .WithAddedMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn), labels))
            .WithAddedOptions(GithubRunsOn.MatrixDefined);
}
