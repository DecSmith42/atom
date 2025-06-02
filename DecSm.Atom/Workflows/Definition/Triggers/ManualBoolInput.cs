namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a manually-triggered workflow input that accepts a boolean value.
/// </summary>
/// <param name="Name">The name of the input parameter. This will be used to identify the input when the workflow is triggered.</param>
/// <param name="Description">A human-readable description of the input parameter, explaining its purpose.</param>
/// <param name="Required">A value indicating whether this input parameter must be provided when the workflow is triggered.</param>
/// <param name="DefaultValue">The default value for the input parameter if no value is explicitly provided. Can be null, true, or false.</param>
[PublicAPI]
public sealed record ManualBoolInput(string Name, string Description, bool Required, bool? DefaultValue = null)
    : ManualInput(Name, Description, Required)
{
    /// <summary>
    ///     Creates a new <see cref="ManualBoolInput" /> instance from a <see cref="ParamDefinition" />.
    /// </summary>
    /// <param name="paramDefinition">The parameter definition to create the input from.</param>
    /// <param name="required">
    ///     Overrides the required status. If null, the required status is inferred:
    ///     true if <paramref name="paramDefinition" />.DefaultValue is null or empty, false otherwise.
    /// </param>
    /// <returns>A new <see cref="ManualBoolInput" /> instance.</returns>
    /// <remarks>
    ///     The <see cref="DefaultValue" /> is parsed from <paramref name="paramDefinition" />.DefaultValue.
    ///     Ensure <paramref name="paramDefinition" />.DefaultValue is a valid boolean string (e.g., "true", "false") if not null.
    /// </remarks>
    public static ManualBoolInput ForParam(ParamDefinition paramDefinition, bool? required = null) =>
        new(paramDefinition.ArgName,
            paramDefinition.Description,
            required ?? paramDefinition.DefaultValue is not { Length: > 0 },
            paramDefinition.DefaultValue is null
                ? null
                : bool.Parse(paramDefinition.DefaultValue));
}
