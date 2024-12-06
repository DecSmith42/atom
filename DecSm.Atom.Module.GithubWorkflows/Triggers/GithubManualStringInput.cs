namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

[PublicAPI]
public sealed record GithubManualStringInput(string Name, string Description, bool Required, string? DefaultValue = null)
    : ManualInput(Name, Description, Required)
{
    public static GithubManualStringInput ForParam(ParamDefinition paramDefinition, bool? required = null) =>
        new(paramDefinition.Attribute.ArgName,
            paramDefinition.Attribute.Description,
            required ?? paramDefinition.Attribute.DefaultValue is not { Length: > 0 },
            paramDefinition.Attribute.DefaultValue);
}
