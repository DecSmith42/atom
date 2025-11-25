namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
public static class Extensions
{
    extension(WorkflowTargetDefinition workflowTargetDefinition)
    {
        public WorkflowTargetDefinition WithGithubRunnerMatrix(string[] labels) =>
            workflowTargetDefinition
                .WithMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
                {
                    Values = labels,
                })
                .WithOptions(GithubRunsOn.SetByMatrix);

        public WorkflowTargetDefinition WithGithubTokenInjection() =>
            workflowTargetDefinition.WithOptions(WorkflowSecretInjection.Create(nameof(IGithubHelper.GithubToken)));
    }
}
