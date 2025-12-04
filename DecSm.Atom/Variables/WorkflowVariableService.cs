namespace DecSm.Atom.Variables;

/// <summary>
///     Provides a centralized service for managing workflow variables in the Atom build system.
///     This service coordinates variable operations across multiple providers using a chain of responsibility pattern,
///     ensuring that variables can be persisted and retrieved from various storage systems during workflow execution.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="IWorkflowVariableService" /> serves as the primary interface for variable management
///         within the Atom build system. It abstracts the complexity of multiple variable providers and provides
///         a unified API for reading and writing variables across different execution contexts.
///     </para>
///     <para>
///         The service operates by delegating variable operations to registered <see cref="IWorkflowVariableProvider" />
///         implementations. Custom providers are tried first, followed by the base Atom provider as a fallback.
///         This design allows for extensible integration with external systems while maintaining reliable defaults.
///     </para>
///     <para>
///         Variable names are automatically resolved to their corresponding argument names using the build definition's
///         parameter definitions, ensuring consistency between parameter declarations and variable storage.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // Injecting and using the service
/// public class MyBuildTarget
/// {
///     private readonly IWorkflowVariableService _variableService;
///     public MyBuildTarget(IWorkflowVariableService variableService)
///     {
///         _variableService = variableService;
///     }
///     public async Task ExecuteAsync()
///     {
///         // Write a variable for later use
///         await _variableService.WriteVariable("BuildNumber", "1.2.3");
///         // Read a variable from a previous job
///         await _variableService.ReadVariable("setup-job", "Configuration");
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="IWorkflowVariableProvider" />
/// <seealso cref="IVariablesHelper" />
[PublicAPI]
public interface IWorkflowVariableService
{
    /// <summary>
    ///     Writes a variable to the workflow context, making it available for subsequent workflow steps and jobs.
    /// </summary>
    /// <param name="variableName">
    ///     The name of the variable as defined in the build parameter definitions. This name will be
    ///     resolved to the corresponding argument name before being passed to the underlying providers.
    /// </param>
    /// <param name="variableValue">
    ///     The value to store for the variable. Values are persisted as strings and should be
    ///     serialized appropriately if complex data types need to be stored.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous write operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="variableName" /> is null or empty.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown when <paramref name="variableName" /> is not found in the build definition's parameter definitions.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when no providers are available or all providers fail to handle the write operation.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         This method attempts to write the variable using custom providers first, in registration order.
    ///         If no custom provider successfully handles the operation, the base Atom provider is used as a fallback.
    ///         The operation succeeds when any provider successfully persists the variable.
    ///     </para>
    ///     <para>
    ///         The variable name is automatically resolved to its corresponding argument name using the build
    ///         definition's parameter definitions. This ensures consistency between how parameters are declared
    ///         in build definitions and how they are stored in the workflow context.
    ///     </para>
    ///     <para>
    ///         Variables written through this service are typically available for reading in subsequent
    ///         workflow steps, jobs, or even across different workflow runs, depending on the provider implementation.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Write build information variables
    /// await WriteVariable("BuildId", Guid.NewGuid().ToString());
    /// await WriteVariable("BuildVersion", "2.1.0");
    /// await WriteVariable("BuildTimestamp", DateTime.UtcNow.ToString("O"));
    /// // Write computed paths
    /// await WriteVariable("OutputDirectory", "/builds/artifacts");
    /// // Write configuration values
    /// await WriteVariable("Environment", "Production");
    /// </code>
    /// </example>
    Task WriteVariable(string variableName, string variableValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads a variable from the workflow context for a specific job, making it available in the current execution
    ///     context.
    /// </summary>
    /// <param name="jobName">
    ///     The name of the job context from which to read the variable. This allows variables to be
    ///     shared between different jobs within the same workflow execution.
    /// </param>
    /// <param name="variableName">
    ///     The name of the variable as defined in the build parameter definitions. This name will be
    ///     resolved to the corresponding argument name before being passed to the underlying providers.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous read operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="jobName" /> or <paramref name="variableName" /> is null or empty.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown when <paramref name="variableName" /> is not found in the build definition's parameter definitions.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when no providers are available or all providers fail to locate the requested variable.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         This method attempts to read the variable using custom providers first, in registration order.
    ///         If no custom provider successfully locates the variable, the base Atom provider is used as a fallback.
    ///         The operation succeeds when any provider successfully retrieves the variable.
    ///     </para>
    ///     <para>
    ///         The variable name is automatically resolved to its corresponding argument name using the build
    ///         definition's parameter definitions, ensuring consistency with how the variable was originally stored.
    ///     </para>
    ///     <para>
    ///         The job name parameter enables cross-job variable sharing within workflow executions. Variables
    ///         written in one job can be read in subsequent jobs, allowing for data flow and state sharing
    ///         across the workflow pipeline.
    ///     </para>
    ///     <para>
    ///         When a variable is successfully read, it typically becomes available in the current execution
    ///         context, often as environment variables or parameter values, depending on the provider implementation.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Read variables from a setup job
    /// await ReadVariable("setup", "BuildId");
    /// await ReadVariable("setup", "BuildVersion");
    /// // Read configuration from a previous step
    /// await ReadVariable("configure", "TargetEnvironment");
    /// // Read computed values from analysis job
    /// await ReadVariable("analyze", "TestCoverage");
    /// await ReadVariable("analyze", "CodeQualityScore");
    /// </code>
    /// </example>
    Task ReadVariable(string jobName, string variableName, CancellationToken cancellationToken = default);
}

internal sealed class WorkflowVariableService(
    IEnumerable<IWorkflowVariableProvider> workflowVariableProviders,
    IBuildDefinition buildDefinition
) : IWorkflowVariableService
{
    // ReSharper disable once PossibleMultipleEnumeration - Once-only operation
    private readonly AtomWorkflowVariableProvider _baseProvider = workflowVariableProviders
        .OfType<AtomWorkflowVariableProvider>()
        .Single();

    // ReSharper disable once PossibleMultipleEnumeration - Once-only operation
    private readonly IWorkflowVariableProvider[] _customProviders = workflowVariableProviders
        .Where(x => x is not AtomWorkflowVariableProvider)
        .ToArray();

    public async Task WriteVariable(
        string variableName,
        string variableValue,
        CancellationToken cancellationToken = default)
    {
        var variableArgName = buildDefinition.ParamDefinitions[variableName].ArgName;

        foreach (var provider in _customProviders)
            if (await provider.WriteVariable(variableArgName, variableValue, cancellationToken))
                return;

        await _baseProvider.WriteVariable(variableArgName, variableValue, cancellationToken);
    }

    public async Task ReadVariable(string jobName, string variableName, CancellationToken cancellationToken = default)
    {
        var variableArgName = buildDefinition.ParamDefinitions[variableName].ArgName;

        foreach (var provider in _customProviders)
            if (await provider.ReadVariable(jobName, variableArgName, cancellationToken))
                return;

        await _baseProvider.ReadVariable(jobName, variableArgName, cancellationToken);
    }
}
