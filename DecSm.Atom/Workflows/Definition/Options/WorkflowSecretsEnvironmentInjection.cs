namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     Represents a workflow option for injecting secrets into the environment during workflow execution.
///     This option allows workflows to securely access sensitive configuration values through environment variables.
/// </summary>
/// <remarks>
///     <para>
///         This class inherits from <see cref="WorkflowOption{TValue, TOption}" /> with string values,
///         enabling the configuration of secret injection parameters for workflow environments.
///     </para>
///     <para>
///         Multiple instances of this option can be applied to a single workflow, allowing for
///         the injection of multiple secrets simultaneously. Each instance represents a separate
///         secret that will be made available as an environment variable during workflow execution.
///     </para>
///     <para>
///         Security considerations:
///         - Secrets are injected as environment variables and should be handled with appropriate care
///         - The actual secret values are not stored in this configuration object
///         - Secret resolution and injection occurs at runtime through the workflow execution engine
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // Example usage in workflow configuration
/// var secretOption = new WorkflowSecretsEnvironmentInjection
/// {
///     Value = "DATABASE_CONNECTION_STRING"
/// };
/// // Multiple secrets can be injected
/// var secrets = new[]
/// {
///     new WorkflowSecretsEnvironmentInjection { Value = "API_KEY" },
///     new WorkflowSecretsEnvironmentInjection { Value = "DATABASE_PASSWORD" },
///     new WorkflowSecretsEnvironmentInjection { Value = "ENCRYPTION_KEY" }
/// };
/// </code>
/// </example>
[PublicAPI]
public sealed record WorkflowSecretsEnvironmentInjection : WorkflowOption<string, WorkflowSecretsEnvironmentInjection>
{
    public override bool AllowMultiple => true;
}
