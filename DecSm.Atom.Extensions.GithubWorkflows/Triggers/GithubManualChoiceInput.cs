namespace DecSm.Atom.Extensions.GithubWorkflows.Triggers;

public sealed record GithubManualChoiceInput(
    string Name,
    string Description,
    bool Required,
    IReadOnlyList<string> Choices,
    string? DefaultValue = null
) : ManualInput(Name, Description, Required)
{
    public static GithubManualChoiceInput ForParam(ParamDefinition paramDefinition, bool required, IReadOnlyList<string> choices) =>
        new(paramDefinition.Attribute.ArgName,
            paramDefinition.Attribute.Description,
            required,
            choices,
            paramDefinition.Attribute.DefaultValue);
}