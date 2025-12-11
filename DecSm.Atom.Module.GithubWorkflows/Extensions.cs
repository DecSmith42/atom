namespace DecSm.Atom.Module.GithubWorkflows;

/// <summary>
///     Provides extension methods for <see cref="WorkflowTargetDefinition" /> to simplify GitHub Actions workflow
///     configuration.
/// </summary>
[PublicAPI]
public static class Extensions
{
    /// <summary>
    ///     Extension methods for <see cref="WorkflowTargetDefinition" />.
    /// </summary>
    extension(WorkflowTargetDefinition workflowTargetDefinition)
    {
        /// <summary>
        ///     Configures the workflow target to run on a matrix of GitHub Actions runner labels.
        /// </summary>
        /// <param name="labels">An array of runner labels (e.g., "ubuntu-latest", "windows-latest").</param>
        /// <returns>The modified <see cref="WorkflowTargetDefinition" /> for chaining.</returns>
        /// <remarks>
        ///     This method sets up a matrix dimension for the `JobRunsOn` parameter, allowing the job
        ///     to execute on multiple runner environments. It also adds the <see cref="GithubRunsOn.SetByMatrix" />
        ///     option to indicate that the runner is determined by the matrix.
        /// </remarks>
        [PublicAPI]
        public WorkflowTargetDefinition WithGithubRunnerMatrix(string[] labels) =>
            workflowTargetDefinition
                .WithMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
                {
                    Values = labels,
                })
                .WithOptions(GithubRunsOn.SetByMatrix);

        /// <summary>
        ///     Configures the workflow target to inject the GitHub token as a secret.
        /// </summary>
        /// <returns>The modified <see cref="WorkflowTargetDefinition" /> for chaining.</returns>
        /// <remarks>
        ///     This method adds a <see cref="WorkflowSecretInjection" /> option for the <see cref="IGithubHelper.GithubToken" />,
        ///     making the GitHub Actions token available as a secret within the workflow job.
        /// </remarks>
        [PublicAPI]
        public WorkflowTargetDefinition WithGithubTokenInjection(GithubTokenPermissionsOption? permissions = null) =>
            permissions is not null
                ? workflowTargetDefinition.WithOptions(
                    WorkflowSecretInjection.Create(nameof(IGithubHelper.GithubToken)),
                    permissions)
                : workflowTargetDefinition.WithOptions(
                    WorkflowSecretInjection.Create(nameof(IGithubHelper.GithubToken)));

        /// <summary>
        ///     Configures the workflow target to set specific permissions for the injected GitHub token.
        /// </summary>
        /// <param name="permissions">The permissions to assign to the GitHub Actions token for this workflow job.</param>
        /// <returns>The modified <see cref="WorkflowTargetDefinition" /> for chaining.</returns>
        /// <remarks>
        ///     This method adds a <see cref="GithubTokenPermissionsOption" /> to the workflow target, allowing you to
        ///     specify fine-grained permissions for the GitHub Actions token used in the job.
        ///     <para>
        ///         Example:
        ///         <code>
        ///         target.WithGithubTokenPermissions(GithubTokenPermissionsOption.ReadPackages);
        ///         </code>
        ///     </para>
        /// </remarks>
        [PublicAPI]
        public WorkflowTargetDefinition WithGithubTokenPermissions(GithubTokenPermissionsOption permissions) =>
            workflowTargetDefinition.WithOptions(permissions);
    }
}
