namespace DecSm.Atom.Variables;

/// <summary>
///     Defines a provider interface for reading and writing workflow variables in the Atom build system.
///     Implementations of this interface handle the persistence and retrieval of variables across
///     different workflow execution contexts and job boundaries.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="IWorkflowVariableProvider" /> interface is part of the extensible variable management
///         system in Atom. Multiple providers can be registered and the <see cref="IWorkflowVariableService" />
///         will delegate operations to these providers in a chain of responsibility pattern.
///     </para>
///     <para>
///         Custom providers can implement this interface to integrate with external variable storage systems
///         such as CI/CD platforms (GitHub Actions, Azure DevOps), configuration management tools, or
///         custom storage solutions. The built-in <c>AtomWorkflowVariableProvider</c> serves as the default
///         fallback provider.
///     </para>
///     <para>
///         Providers should return <c>true</c> from their methods when they successfully handle the operation,
///         or <c>false</c> to indicate that the operation should be delegated to the next provider in the chain.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// public class CustomVariableProvider : IWorkflowVariableProvider
/// {
///     public async Task&lt;bool&gt; WriteVariable(string variableName, string variableValue)
///     {
///         // Custom logic to write variable to external system
///         if (await WriteToExternalSystem(variableName, variableValue))
///         {
///             return true; // Successfully handled
///         }
///         return false; // Let next provider handle it
///     }
/// 
///     public async Task&lt;bool&gt; ReadVariable(string jobName, string variableName)
///     {
///         // Custom logic to read variable from external system
///         var value = await ReadFromExternalSystem(jobName, variableName);
///         return value != null; // Return true if found
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="IWorkflowVariableService" />
/// <seealso cref="IVariablesHelper" />
public interface IWorkflowVariableProvider
{
    /// <summary>
    ///     Writes a variable with the specified name and value to the provider's storage system.
    /// </summary>
    /// <param name="variableName">
    ///     The name of the variable to write. This is typically the argument name as defined
    ///     in the build parameter definitions, not the original parameter name.
    /// </param>
    /// <param name="variableValue">
    ///     The value to store for the variable. Values are always stored as strings and
    ///     should be serialized appropriately by the calling code if complex types are involved.
    /// </param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> that represents the asynchronous write operation.
    ///     The task result is <c>true</c> if this provider successfully handled the write operation;
    ///     <c>false</c> if the operation should be delegated to the next provider in the chain.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="variableName" /> is null or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the provider encounters an error that prevents it from determining
    ///     whether it can handle the operation (as opposed to simply declining to handle it).
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         Implementations should return <c>true</c> only when they have successfully persisted
    ///         the variable to their storage system. Returning <c>false</c> indicates that this
    ///         provider cannot or chooses not to handle this particular variable, allowing the
    ///         workflow variable service to try the next provider in the chain.
    ///     </para>
    ///     <para>
    ///         The variable name passed to this method is the processed argument name from the
    ///         parameter definition, which may differ from the original parameter name used in
    ///         build definitions.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Example implementation for a GitHub Actions provider
    /// public async Task&lt;bool&gt; WriteVariable(string variableName, string variableValue)
    /// {
    ///     if (!IsGitHubActionsEnvironment())
    ///         return false; // Not our environment, let another provider handle it
    /// 
    ///     await WriteToGitHubOutput(variableName, variableValue);
    ///     return true; // Successfully written to GitHub Actions output
    /// }
    /// </code>
    /// </example>
    public Task<bool> WriteVariable(string variableName, string variableValue);

    /// <summary>
    ///     Reads a variable from the provider's storage system for a specific job context.
    /// </summary>
    /// <param name="jobName">
    ///     The name of the job context from which to read the variable. This allows
    ///     providers to scope variables to specific job executions or workflow runs.
    /// </param>
    /// <param name="variableName">
    ///     The name of the variable to read. This is typically the argument name as defined
    ///     in the build parameter definitions, not the original parameter name.
    /// </param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> that represents the asynchronous read operation.
    ///     The task result is <c>true</c> if this provider successfully found and retrieved
    ///     the variable; <c>false</c> if the variable was not found in this provider's storage
    ///     or if this provider cannot handle the read operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="jobName" /> or <paramref name="variableName" /> is null or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the provider encounters an error that prevents it from determining
    ///     whether the variable exists (as opposed to simply not finding it).
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         Implementations should return <c>true</c> only when they have successfully located
    ///         and retrieved the requested variable from their storage system. Returning <c>false</c>
    ///         indicates that this provider does not have the variable or cannot handle the read
    ///         operation, allowing the workflow variable service to try the next provider in the chain.
    ///     </para>
    ///     <para>
    ///         The job name parameter allows providers to implement job-scoped variable storage,
    ///         where variables written in one job context can be read in subsequent jobs or steps
    ///         within the same workflow execution.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Example implementation for reading from a cache-based provider
    /// public async Task&lt;bool&gt; ReadVariable(string jobName, string variableName)
    /// {
    ///     var cacheKey = $"{jobName}:{variableName}";
    ///     var cachedValue = await _cache.GetAsync(cacheKey);
    /// 
    ///     if (cachedValue != null)
    ///     {
    ///         // Variable found in cache, make it available to the workflow
    ///         await SetWorkflowVariable(variableName, cachedValue);
    ///         return true;
    ///     }
    /// 
    ///     return false; // Not found in this provider
    /// }
    /// </code>
    /// </example>
    public Task<bool> ReadVariable(string jobName, string variableName);
}
