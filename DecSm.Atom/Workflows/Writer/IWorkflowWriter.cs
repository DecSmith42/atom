namespace DecSm.Atom.Workflows.Writer;

/// <summary>
///     Defines a contract for workflow writers that can generate and validate workflow files.
///     Workflow writers are responsible for translating workflow models into platform-specific
///     workflow files (e.g., GitHub Actions, Azure DevOps, etc.).
/// </summary>
/// <remarks>
///     <para>
///         This interface is part of a strategy pattern implementation where different workflow
///         writers handle different platform types. Writers are typically registered as singletons
///         in the dependency injection container and discovered at runtime.
///     </para>
///     <para>
///         Implementations are expected to be thread-safe as they may be used concurrently
///         across multiple workflow generation operations.
///     </para>
/// </remarks>
[PublicAPI]
public interface IWorkflowWriter
{
    /// <summary>
    ///     Gets the type of workflow that this writer can handle.
    /// </summary>
    /// <value>
    ///     A <see cref="Type" /> that implements <see cref="IWorkflowType" />, indicating
    ///     which specific workflow platform this writer targets.
    /// </value>
    /// <remarks>
    ///     This property is used by the workflow generation system to match writers
    ///     with appropriate workflow models. Each writer should handle exactly one
    ///     workflow type.
    /// </remarks>
    Type WorkflowType { get; }

    /// <summary>
    ///     Generates a workflow file from the specified workflow model.
    /// </summary>
    /// <param name="workflow">The workflow model containing the configuration and steps to generate.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous generation operation.</returns>
    /// <remarks>
    ///     <para>
    ///         This method translates the platform-agnostic workflow model into a platform-specific
    ///         workflow file format. The actual file location and format depend on the specific
    ///         implementation and target platform.
    ///     </para>
    ///     <para>
    ///         Implementations should handle file I/O operations asynchronously and may perform
    ///         additional validation or transformation of the workflow model during generation.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="workflow" /> is null.</exception>
    Task Generate(WorkflowModel workflow, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether the current workflow file differs from what would be generated
    ///     from the specified workflow model.
    /// </summary>
    /// <param name="workflow">The workflow model to compare against the existing file.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A task containing <c>true</c> if the existing workflow file is outdated or missing
    ///     and needs regeneration; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This method enables incremental workflow generation by allowing the system to
    ///         skip regeneration when the workflow file is already up-to-date. Implementations
    ///         typically compare the content that would be generated against the existing file.
    ///     </para>
    ///     <para>
    ///         A return value of <c>true</c> indicates that calling <see cref="Generate" />
    ///         would result in changes to the workflow file.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="workflow" /> is null.</exception>
    Task<bool> CheckForDirtyWorkflow(WorkflowModel workflow, CancellationToken cancellationToken = default);
}

/// <summary>
///     Provides a strongly-typed version of <see cref="IWorkflowWriter" /> for a specific workflow type.
/// </summary>
/// <typeparam name="T">
///     The specific workflow type that this writer handles, which must implement
///     <see cref="IWorkflowType" />.
/// </typeparam>
/// <remarks>
///     <para>
///         This generic interface provides compile-time type safety by constraining the workflow type
///         that the writer can handle. It automatically implements the <see cref="IWorkflowWriter.WorkflowType" />
///         property using the generic type parameter.
///     </para>
///     <para>
///         Implementations should inherit from this generic interface rather than implementing
///         <see cref="IWorkflowWriter" /> directly to benefit from type safety and automatic
///         workflow type resolution.
///     </para>
/// </remarks>
[PublicAPI]
public interface IWorkflowWriter<T> : IWorkflowWriter
    where T : IWorkflowType
{
    /// <summary>
    ///     Gets the workflow type, automatically resolved from the generic type parameter.
    /// </summary>
    /// <value>The <see cref="Type" /> of <typeparamref name="T" />.</value>
    Type IWorkflowWriter.WorkflowType => typeof(T);

    /// <summary>
    ///     Generates a workflow file from the specified workflow model.
    /// </summary>
    /// <param name="workflow">The workflow model containing the configuration and steps to generate.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous generation operation.</returns>
    /// <remarks>
    ///     Implementing classes must provide a concrete implementation of this method
    ///     that handles the specific workflow format for type <typeparamref name="T" />.
    /// </remarks>
    abstract Task IWorkflowWriter.Generate(WorkflowModel workflow, CancellationToken cancellationToken);

    /// <summary>
    ///     Checks whether the current workflow file differs from what would be generated
    ///     from the specified workflow model.
    /// </summary>
    /// <param name="workflow">The workflow model to compare against the existing file.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A task containing <c>true</c> if the existing workflow file needs regeneration;
    ///     otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Implementing classes must provide a concrete implementation of this method
    ///     that performs the dirty check for the specific workflow format of type <typeparamref name="T" />.
    /// </remarks>
    abstract Task<bool> IWorkflowWriter.CheckForDirtyWorkflow(
        WorkflowModel workflow,
        CancellationToken cancellationToken);
}
