namespace DecSm.Atom.Workflows;

/// <summary>
///     Defines a contract for providing workflow options within the Atom workflow system.
///     This interface enables classes to expose a collection of workflow options that can be
///     consumed by workflow engines, builders, or configuration systems.
/// </summary>
/// <remarks>
///     <para>
///         Implementing classes should ensure that the <see cref="WorkflowOptions" /> property
///         returns a stable, immutable collection of workflow options. The collection should
///         not change during the lifetime of the provider instance unless explicitly designed
///         to support dynamic option loading.
///     </para>
///     <para>
///         This interface follows the provider pattern, allowing for dependency injection
///         and loose coupling between workflow option sources and consumers.
///     </para>
///     <para>
///         The interface is marked with <see cref="JetBrains.Annotations.PublicAPIAttribute" /> indicating it is part of
///         the public API surface and should maintain backward compatibility.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// public class DefaultWorkflowOptionProvider : IWorkflowOptionProvider
/// {
///     private readonly IReadOnlyList&lt;IWorkflowOption&gt; _options;
///     public DefaultWorkflowOptionProvider(IEnumerable&lt;IWorkflowOption&gt; options)
///     {
///         _options = options?.ToList()?.AsReadOnly() ??
///                   throw new ArgumentNullException(nameof(options));
///     }
///     public IReadOnlyList&lt;IWorkflowOption&gt; WorkflowOptions =&gt; _options;
/// }
/// </code>
/// </example>
[PublicAPI]
public interface IWorkflowOptionProvider
{
    /// <summary>
    ///     Gets a read-only collection of workflow options provided by this instance.
    /// </summary>
    /// <value>
    ///     A read-only list containing zero or more <see cref="IWorkflowOption" /> instances.
    ///     The collection should never be null, but may be empty if no options are available.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         The returned collection should be treated as immutable by consumers. If dynamic
    ///         options are required, implementers should return a new collection instance
    ///         rather than modifying the existing one.
    ///     </para>
    ///     <para>
    ///         Implementers should ensure thread-safety if the provider will be used in
    ///         multi-threaded scenarios. Consider using immutable collections or proper
    ///         synchronization mechanisms.
    ///     </para>
    ///     <para>
    ///         The order of options in the collection may be significant depending on the
    ///         consuming workflow system. Implementers should document any ordering requirements
    ///         or guarantees.
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     May be thrown by implementers if the provider is in an invalid state
    ///     or if options cannot be retrieved due to configuration errors.
    /// </exception>
    IReadOnlyList<IWorkflowOption> WorkflowOptions { get; }
}
