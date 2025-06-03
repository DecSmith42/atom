namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     Represents a workflow option that injects environment variables into the workflow execution context.
///     This allows workflows to access custom environment variables that are not part of the default runtime environment.
/// </summary>
/// <remarks>
///     <para>
///         This sealed record provides a mechanism for injecting environment variables into workflow executions
///         across different workflow platforms (GitHub Actions, Azure DevOps, etc.). Environment variables
///         injected through this option become available to all steps within the workflow execution.
///     </para>
///     <para>
///         The class inherits from <see cref="WorkflowOption{TData, TSelf}" /> with <c>string</c> as the data type,
///         where the <see cref="WorkflowOption{TData, TSelf}.Value" /> property contains the environment variable value
///         to be injected.
///     </para>
///     <para>
///         Environment injections are commonly used for:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>Configuration values that need to be available across workflow steps</description>
///         </item>
///         <item>
///             <description>Build-time constants and settings</description>
///         </item>
///         <item>
///             <description>Feature flags and conditional execution parameters</description>
///         </item>
///         <item>
///             <description>Platform-specific configuration values</description>
///         </item>
///     </list>
///     <para>
///         For sensitive values such as API keys, passwords, or tokens, consider using
///         <see cref="WorkflowSecretsEnvironmentInjection" /> instead, which provides secure handling
///         of sensitive environment variables.
///     </para>
///     <para>
///         The workflow generators (such as <c>GithubWorkflowWriter</c> and <c>DevopsWorkflowWriter</c>)
///         automatically process these injections and configure the appropriate environment variable
///         mechanisms for their respective platforms.
///     </para>
/// </remarks>
/// <example>
///     <para>Creating and using environment variable injections:</para>
///     <code>
/// // Create environment variable injections
/// var buildConfiguration = new WorkflowEnvironmentInjection
/// {
///     Name = "BUILD_CONFIGURATION",
///     Value = "Release"
/// };
/// var apiBaseUrl = new WorkflowEnvironmentInjection
/// {
///     Name = "API_BASE_URL",
///     Value = "https://api.example.com"
/// };
/// // Add to workflow configuration
/// var workflowDefinition = new WorkflowDefinition()
///     .WithAddedOptions(buildConfiguration, apiBaseUrl);
/// // The environment variables will be available in workflow steps as:
/// // $BUILD_CONFIGURATION (GitHub Actions) or $(BUILD_CONFIGURATION) (Azure DevOps)
/// // $API_BASE_URL (GitHub Actions) or $(API_BASE_URL) (Azure DevOps)
/// </code>
/// </example>
/// <seealso cref="WorkflowOption{TData, TSelf}" />
/// <seealso cref="WorkflowSecretsEnvironmentInjection" />
/// <seealso cref="WorkflowParamInjection" />
/// <seealso cref="IWorkflowOption" />
[PublicAPI]
public sealed record WorkflowEnvironmentInjection : WorkflowOption<string, WorkflowEnvironmentInjection>
{
    public override bool AllowMultiple => true;
}
