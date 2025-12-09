namespace DecSm.Atom.Workflows.Definition.Options;

/// <summary>
///     An abstract base record for creating strongly-typed, data-carrying workflow options.
/// </summary>
/// <typeparam name="TData">The type of data this option carries.</typeparam>
/// <typeparam name="TSelf">The concrete type implementing this option, used for fluent APIs.</typeparam>
/// <remarks>
///     This class serves as the foundation for all data-carrying workflow options, implementing
///     <see cref="IWorkflowOption" />
///     while providing type-safe data handling. The self-referencing generic pattern ensures that methods on derived types
///     return the correct concrete type.
/// </remarks>
/// <example>
///     <code>
/// public sealed record BuildConfiguration : WorkflowOption&lt;string, BuildConfiguration&gt;;
/// // Usage:
/// var config = BuildConfiguration.Create("Release");
///     </code>
/// </example>
[PublicAPI]
public abstract record WorkflowOption<TData, TSelf> : IWorkflowOption
    where TSelf : WorkflowOption<TData, TSelf>, new()
{
    /// <summary>
    ///     Gets the data value carried by this workflow option.
    /// </summary>
    public virtual TData? Value { get; init; }

    /// <summary>
    ///     Gets a value indicating whether multiple instances of this option are allowed in a workflow.
    ///     Defaults to <c>false</c>.
    /// </summary>
    public virtual bool AllowMultiple => false;

    /// <summary>
    ///     Creates a new instance of the workflow option with the specified value.
    /// </summary>
    /// <param name="value">The value to assign to the option.</param>
    /// <returns>A new instance of the workflow option.</returns>
    public static TSelf Create(TData value) =>
        new()
        {
            Value = value,
        };
}
