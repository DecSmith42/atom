namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a manual input in a workflow where the user must select a value from a predefined list of choices.
/// </summary>
/// <param name="Name">The name of the input field. This is typically used as an identifier.</param>
/// <param name="Description">A human-readable description of what the input is for.</param>
/// <param name="Required">A boolean value indicating whether this input is mandatory.</param>
/// <param name="Choices">A read-only list of string options that the user can choose from.</param>
/// <param name="DefaultValue">An optional default value that will be pre-selected. If null, no default value is set.</param>
[PublicAPI]
public sealed record ManualChoiceInput(
    string Name,
    string Description,
    bool Required,
    IReadOnlyList<string> Choices,
    string? DefaultValue = null
) : ManualInput(Name, Description, Required)
{
    /// <summary>
    ///     Creates a <see cref="ManualChoiceInput" /> instance based on a <see cref="ParamDefinition" />.
    /// </summary>
    /// <param name="paramDefinition">The parameter definition to create the input from.</param>
    /// <param name="choices">The list of choices for this input.</param>
    /// <param name="required">
    ///     An optional boolean to override the required status.
    ///     If not provided, the input is considered required if <see cref="ParamDefinition.DefaultValue" /> is null or empty.
    /// </param>
    /// <returns>A new instance of <see cref="ManualChoiceInput" />.</returns>
    public static ManualChoiceInput ForParam(ParamDefinition paramDefinition, IReadOnlyList<string> choices, bool? required = null) =>
        new(paramDefinition.ArgName,
            paramDefinition.Description,
            required ?? paramDefinition.DefaultValue is not { Length: > 0 },
            choices,
            paramDefinition.DefaultValue);
}
