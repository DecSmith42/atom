namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
public static class Extensions
{
    public static WorkflowTargetDefinition WithGithubRunnerMatrix(
        this WorkflowTargetDefinition workflowTargetDefinition,
        string[] labels) =>
        workflowTargetDefinition
            .WithAddedMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn), labels))
            .WithAddedOptions(GithubRunsOn.SetByMatrix);

    public static WorkflowTargetDefinition WithGithubTokenInjection(this WorkflowTargetDefinition workflowTargetDefinition) =>
        workflowTargetDefinition.WithAddedOptions(WorkflowSecretInjection.Create(nameof(IGithubHelper.GithubToken)));
}
