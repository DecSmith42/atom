namespace DecSm.Atom.Variables;

/// <summary>
///     Provides helper methods for managing workflow variables within the Atom build system.
///     This interface enables components to write variables that can be shared across different
///     build steps and targets in the workflow execution context.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="IVariablesHelper" /> interface extends <see cref="IBuildAccessor" /> to provide
///         access to the service container while offering convenient methods for variable management.
///         Variables written through this interface are persisted in the workflow context and can be
///         consumed by subsequent build steps or targets.
///     </para>
///     <para>
///         This interface is typically implemented by build definition interfaces that need to share
///         data between different execution phases, such as build information, configuration values,
///         or computed results that other targets depend on.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // Writing variables in a target execution
/// await WriteVariable("BuildId", "12345");
/// await WriteVariable("BuildVersion", "1.0.0");
/// await WriteVariable("BuildTimestamp", DateTime.UtcNow.ToString());
/// </code>
/// </example>
/// <seealso cref="IBuildAccessor" />
/// <seealso cref="IWorkflowVariableService" />
public interface IVariablesHelper : IBuildAccessor
{
    /// <summary>
    ///     Writes a variable to the workflow context, making it available for later steps.
    /// </summary>
    /// <param name="name">
    ///     The name of the variable. This should be a unique identifier that other
    ///     build steps can use to retrieve the variable value.
    /// </param>
    /// <param name="value">
    ///     The value of the variable. The value will be stored as a string in the
    ///     workflow context and can be retrieved by subsequent build steps.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation of writing
    ///     the variable to the workflow context.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="name" /> is null or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the workflow variable service is not available or the
    ///     workflow context is not properly initialized.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         Variables written through this method are persisted in the current workflow
    ///         execution context and remain available throughout the entire build process.
    ///         If a variable with the same name already exists, it will be overwritten
    ///         with the new value.
    ///     </para>
    ///     <para>
    ///         The actual variable writing is delegated to the <see cref="IWorkflowVariableService" />
    ///         which handles the underlying storage and retrieval mechanisms.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Write a build identifier
    /// await WriteVariable("BuildId", Guid.NewGuid().ToString());
    /// // Write version information
    /// await WriteVariable("Version", "2.1.0-beta");
    /// // Write computed values
    /// await WriteVariable("OutputPath", Path.Combine(baseDir, "bin", "Release"));
    /// </code>
    /// </example>
    Task WriteVariable(string name, string value, CancellationToken cancellationToken = default) =>
        GetService<IWorkflowVariableService>()
            .WriteVariable(name, value, cancellationToken);
}
