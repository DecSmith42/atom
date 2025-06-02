namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a trigger that is initiated manually, optionally with a predefined set of inputs.
///     Implements <see cref="IWorkflowTrigger" />.
/// </summary>
/// <param name="Inputs">
///     A read-only list of manual inputs required when the workflow is triggered. Defaults to <c>null</c> if no inputs are
///     specified.
/// </param>
[PublicAPI]
public sealed record ManualTrigger(IReadOnlyList<ManualInput>? Inputs = null) : IWorkflowTrigger
{
    /// <summary>
    ///     Gets an instance of <see cref="ManualTrigger" /> with no inputs.
    ///     This represents a manual trigger that does not require any specific input parameters.
    /// </summary>
    public static ManualTrigger Empty { get; } = new();
}
