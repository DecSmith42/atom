namespace DecSm.Atom.Extensions.GithubWorkflows;

public static class Extensions
{
    public static CommandDefinition WithGithubRunnerMatrix(this CommandDefinition commandDefinition, string[] labels) =>
        commandDefinition
            .WithAddedMatrixDimensions(new MatrixDimension(nameof(IGithubWorkflows.GithubRunsOn), labels))
            .WithAddedOptions(GithubRunsOn.MatrixDefined);
}