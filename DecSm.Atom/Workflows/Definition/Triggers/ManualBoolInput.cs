namespace DecSm.Atom.Workflows.Definition.Triggers;

[PublicAPI]
public sealed record ManualBoolInput(string Name, string Description, bool Required, bool? DefaultValue = null)
    : ManualInput(Name, Description, Required)
{
    public static ManualBoolInput ForParam(ParamDefinition paramDefinition, bool? required = null) =>
        new(paramDefinition.ArgName,
            paramDefinition.Description,
            required ?? paramDefinition.DefaultValue is not { Length: > 0 },
            paramDefinition.DefaultValue is null
                ? null
                : bool.Parse(paramDefinition.DefaultValue));
}
