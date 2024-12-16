namespace DecSm.Atom.Workflows.Definition.Triggers;

[PublicAPI]
public sealed record ManualStringInput(string Name, string Description, bool Required, string? DefaultValue = null)
    : ManualInput(Name, Description, Required)
{
    public static ManualStringInput ForParam(ParamDefinition paramDefinition, bool? required = null) =>
        new(paramDefinition.ArgName,
            paramDefinition.Description,
            required ?? paramDefinition.DefaultValue is not { Length: > 0 },
            paramDefinition.DefaultValue);
}
