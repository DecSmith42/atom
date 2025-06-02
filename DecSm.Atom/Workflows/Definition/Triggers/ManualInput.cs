namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents the base definition for a manual input trigger in a workflow.
///     This abstract record provides common properties for all manual input types.
///     It is intended to be inherited by concrete input types such as text, boolean, or choice inputs.
/// </summary>
/// <param name="Name">The programmatic name of the input. This is often used as an identifier in scripts or configurations.</param>
/// <param name="Description">A user-friendly description of the input, explaining its purpose and what kind of data is expected.</param>
/// <param name="Required">
///     A boolean value indicating whether this input must be provided when the workflow is manually triggered. Defaults to
///     true.
/// </param>
[PublicAPI]
public abstract record ManualInput(string Name, string Description, bool Required = true);
