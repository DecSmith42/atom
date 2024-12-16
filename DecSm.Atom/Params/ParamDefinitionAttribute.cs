namespace DecSm.Atom.Params;

// If the constructor args are modified, the BuildDefinitionSourceGenerator will need to be updated
[PublicAPI]
[AttributeUsage(AttributeTargets.Property)]
public class ParamDefinitionAttribute(
    string argName,
    string description,
    string? defaultValue = null,
    ParamSource sources = ParamSource.All
) : Attribute
{
    public string ArgName => argName;

    public string Description => description;

    public string? DefaultValue => defaultValue;

    public ParamSource Sources => sources;

    public bool IsSecret { get; protected init; }
}
