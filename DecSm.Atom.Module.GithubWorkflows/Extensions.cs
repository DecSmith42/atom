namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
public static class Extensions
{
    public static WorkflowTargetDefinition WithGithubRunnerMatrix(
        this WorkflowTargetDefinition workflowTargetDefinition,
        string[] labels) =>
        workflowTargetDefinition
            .WithMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
            {
                Values = labels,
            })
            .WithOptions(GithubRunsOn.SetByMatrix);

    public static WorkflowTargetDefinition WithGithubTokenInjection(this WorkflowTargetDefinition workflowTargetDefinition) =>
        workflowTargetDefinition.WithOptions(WorkflowSecretInjection.Create(nameof(IGithubHelper.GithubToken)));
}
