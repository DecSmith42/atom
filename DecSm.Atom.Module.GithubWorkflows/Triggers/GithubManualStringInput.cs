namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

public sealed record GithubManualStringInput(string Name, string Description, bool Required, string? DefaultValue = null)
    : ManualInput(Name, Description, Required)
{
    public static GithubManualStringInput ForParam(ParamDefinition paramDefinition, bool required) =>
        new(paramDefinition.Attribute.ArgName, paramDefinition.Attribute.Description, required, paramDefinition.Attribute.DefaultValue);
}
