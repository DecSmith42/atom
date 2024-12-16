namespace DecSm.Atom.Workflows.Definition.Triggers;

[PublicAPI]
public sealed record ManualChoiceInput(
    string Name,
    string Description,
    bool Required,
    IReadOnlyList<string> Choices,
    string? DefaultValue = null
) : ManualInput(Name, Description, Required)
{
    public static ManualChoiceInput ForParam(ParamDefinition paramDefinition, IReadOnlyList<string> choices, bool? required = null) =>
        new(paramDefinition.ArgName,
            paramDefinition.Description,
            required ?? paramDefinition.DefaultValue is not { Length: > 0 },
            choices,
            paramDefinition.DefaultValue);
}
