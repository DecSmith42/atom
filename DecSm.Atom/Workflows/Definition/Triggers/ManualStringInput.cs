namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a manually provided string input for a workflow.
/// </summary>
/// <param name="Name">The programmatic name or identifier for the input.</param>
/// <param name="Description">A human-readable description of the input, explaining its purpose.</param>
/// <param name="Required">A flag indicating whether this input must be provided by the user. True if required, false otherwise.</param>
/// <param name="DefaultValue">An optional default value for the string input if the user does not provide one. Defaults to null.</param>
[PublicAPI]
public sealed record ManualStringInput(string Name, string Description, bool Required, string? DefaultValue = null)
    : ManualInput(Name, Description, Required)
{
    /// <summary>
    ///     Creates a <see cref="ManualStringInput" /> instance based on a <see cref="ParamDefinition" />.
    /// </summary>
    /// <param name="paramDefinition">The parameter definition to base the input on.</param>
    /// <param name="required">
    ///     An optional override for the required status.
    ///     If null, the input is considered required if <paramref name="paramDefinition" />.DefaultValue is null or empty.
    /// </param>
    /// <returns>A new instance of <see cref="ManualStringInput" /> configured from the provided parameter definition.</returns>
    public static ManualStringInput ForParam(ParamDefinition paramDefinition, bool? required = null) =>
        new(paramDefinition.ArgName,
            paramDefinition.Description,
            required ?? paramDefinition.DefaultValue is not { Length: > 0 },
            paramDefinition.DefaultValue);
}
