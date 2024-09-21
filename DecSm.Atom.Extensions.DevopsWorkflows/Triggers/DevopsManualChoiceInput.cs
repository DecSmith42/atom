namespace DecSm.Atom.Extensions.DevopsWorkflows.Triggers;

public sealed record DevopsManualChoiceInput(string Name, string Description, IReadOnlyList<string> Choices, string? DefaultValue = null)
    : ManualInput(Name, Description)
{
    public static DevopsManualChoiceInput ForParam(ParamDefinition paramDefinition, IReadOnlyList<string> choices) =>
        new(paramDefinition.Attribute.ArgName, paramDefinition.Attribute.Description, choices, paramDefinition.Attribute.DefaultValue);
}
