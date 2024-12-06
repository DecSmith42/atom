namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

[PublicAPI]
public sealed record GithubManualBoolInput(string Name, string Description, bool Required, bool? DefaultValue = null)
    : ManualInput(Name, Description, Required)
{
    public static GithubManualBoolInput ForParam(ParamDefinition paramDefinition, bool? required = null) =>
        new(paramDefinition.Attribute.ArgName,
            paramDefinition.Attribute.Description,
            required ?? paramDefinition.Attribute.DefaultValue is not { Length: > 0 },
            paramDefinition.Attribute.DefaultValue is null
                ? null
                : bool.Parse(paramDefinition.Attribute.DefaultValue));
}
