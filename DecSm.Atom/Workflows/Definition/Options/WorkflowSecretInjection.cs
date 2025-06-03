namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     Represents a workflow option that injects secret values into the workflow execution context.
///     Secrets are handled securely by workflow platforms and are not exposed in logs or outputs.
/// </summary>
/// <remarks>
///     <para>
///         This sealed record provides a mechanism for injecting secret values into workflow executions
///         across different workflow platforms (GitHub Actions, Azure DevOps, etc.). Unlike regular
///         parameters or environment variables, secrets are treated with special security considerations.
///     </para>
///     <para>
///         Secret injections are commonly used for:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>API tokens and authentication keys</description>
///         </item>
///         <item>
///             <description>Database connection strings with credentials</description>
///         </item>
///         <item>
///             <description>Signing certificates and private keys</description>
///         </item>
///         <item>
///             <description>Third-party service credentials</description>
///         </item>
///     </list>
///     <para>
///         The workflow generators automatically process these injections and configure the appropriate
///         secret mechanisms for their respective platforms, ensuring values are masked in logs and
///         handled according to security best practices.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // Create secret injection using factory method
/// var githubTokenSecret = WorkflowSecretInjection.Create(nameof(IGithubHelper.GithubToken));
/// // Add to workflow configuration
/// var workflowDefinition = new WorkflowDefinition()
///     .WithAddedOptions(githubTokenSecret);
/// // The secret will be available securely in workflow steps
/// </code>
/// </example>
/// <seealso cref="WorkflowOption{TData, TSelf}" />
/// <seealso cref="WorkflowSecretsSecretInjection" />
/// <seealso cref="WorkflowParamInjection" />
/// <seealso cref="SecretDefinitionAttribute" />
[PublicAPI]
public sealed record WorkflowSecretInjection : WorkflowOption<string, WorkflowSecretInjection>
{
    public override bool AllowMultiple => true;
}
