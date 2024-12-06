namespace DecSm.Atom.Module.GithubWorkflows.Triggers;

[PublicAPI]
public sealed record GithubManualChoiceInput(
    string Name,
    string Description,
    bool Required,
    IReadOnlyList<string> Choices,
    string? DefaultValue = null
) : ManualInput(Name, Description, Required)
{
    public static GithubManualChoiceInput ForParam(ParamDefinition paramDefinition, IReadOnlyList<string> choices, bool? required = null) =>
        new(paramDefinition.Attribute.ArgName,
            paramDefinition.Attribute.Description,
            required ?? paramDefinition.Attribute.DefaultValue is not { Length: > 0 },
            choices,
            paramDefinition.Attribute.DefaultValue);
}
