namespace DecSm.Atom.Workflows.Options;

/// <summary>
///     Represents an abstract base record for defining custom steps within a workflow.
///     Custom steps allow for the extension of workflow capabilities with user-defined operations.
///     This class implements <see cref="IWorkflowOption" />.
/// </summary>
/// <remarks>
///     Inherit from this record to create specific custom workflow steps.
///     For example:
///     <code>
/// public sealed record MyCustomActionStep(string ActionSetting) : CustomStep;
/// </code>
///     Multiple custom steps, including multiple instances of the same step type, can typically be added to a workflow.
/// </remarks>
[PublicAPI]
public abstract record CustomStep : IWorkflowOption
{
    /// <summary>
    ///     Gets or initializes the optional name of the custom step.
    ///     This name can be used for identification or logging purposes within the workflow.
    /// </summary>
    public string? Name { get; init; }

    /// <inheritdoc />
    public bool AllowMultiple => true;
}
