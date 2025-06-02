namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     Represents a workflow option that injects parameter values into the workflow execution context.
///     This allows workflows to override or set specific parameter values that would normally be
///     resolved from various sources like command line arguments, environment variables, or configuration files.
/// </summary>
/// <param name="Name">
///     The name of the parameter to inject. This should correspond to a parameter defined
///     in the workflow system, typically referenced through the <c>Params</c> class or
///     parameter definition attributes.
/// </param>
/// <param name="Value">
///     The value to inject for the specified parameter. This value will take precedence
///     over values from other parameter sources during workflow execution.
/// </param>
/// <remarks>
///     <para>
///         This sealed record provides a mechanism for injecting specific parameter values into workflow
///         executions, allowing fine-grained control over workflow behavior without modifying the
///         underlying parameter resolution system. Parameter injections are processed by workflow
///         generators and become available to workflow steps and build logic.
///     </para>
///     <para>
///         Parameter injections are commonly used for:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>Overriding default parameter values for specific workflows</description>
///         </item>
///         <item>
///             <description>Setting workflow-specific configuration without affecting global settings</description>
///         </item>
///         <item>
///             <description>Providing test or development-specific parameter values</description>
///         </item>
///         <item>
///             <description>Enabling or disabling features through parameter-controlled flags</description>
///         </item>
///     </list>
///     <para>
///         The class implements a custom merging strategy through the <see cref="MergeWith{T}" /> method,
///         which ensures that when multiple injections exist for the same parameter name, the last
///         one takes precedence. This behavior allows for parameter value overriding in a predictable manner.
///     </para>
///     <para>
///         Parameter names should correspond to those defined through <see cref="ParamDefinitionAttribute" />
///         or <see cref="SecretDefinitionAttribute" /> on command properties, ensuring type safety and
///         proper parameter resolution within the workflow execution context.
///     </para>
/// </remarks>
/// <example>
///     <para>Creating and using parameter injections:</para>
///     <code>
/// // Inject a dry-run parameter for testing
/// var dryRunInjection = new WorkflowParamInjection(Params.NugetDryRun, "true");
/// 
/// // Inject a custom configuration value
/// var configInjection = new WorkflowParamInjection("BuildConfiguration", "Debug");
/// 
/// // Add to workflow configuration
/// var workflowDefinition = new WorkflowDefinition()
///     .WithAddedOptions(dryRunInjection, configInjection);
/// 
/// // Multiple injections for the same parameter - last one wins
/// var overrideInjection = new WorkflowParamInjection(Params.NugetDryRun, "false");
/// workflowDefinition = workflowDefinition.WithAddedOptions(overrideInjection);
/// // Final value for NugetDryRun will be "false"
/// </code>
/// </example>
/// <seealso cref="IWorkflowOption" />
/// <seealso cref="ParamDefinitionAttribute" />
/// <seealso cref="SecretDefinitionAttribute" />
/// <seealso cref="WorkflowEnvironmentInjection" />
/// <seealso cref="WorkflowSecretInjection" />
[PublicAPI]
public sealed record WorkflowParamInjection(string Name, string Value) : IWorkflowOption
{
    public bool AllowMultiple => true;

    public static IEnumerable<T> MergeWith<T>(IEnumerable<T> entries)
        where T : IWorkflowOption =>
        entries
            .OfType<WorkflowParamInjection>()
            .GroupBy(x => x.Name)
            .Select(x => x.Last())
            .Cast<T>();
}
