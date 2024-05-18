namespace DecSm.Atom.Params;

[AttributeUsage(AttributeTargets.Property)]
public class ParamAttribute(string argName, string description, string? defaultValue = null) : Attribute
{
    public string ArgName => argName;
    public string Description => description;
    public string? DefaultValue => defaultValue;

    public bool SourceFromCliArguments { get; set; } = true;
    public bool SourceFromEnvironmentVariables { get; set; } = true;
    public bool SourceFromConfigurationFiles { get; set; } = true;
    public bool SourceFromSecrets { get; set; } = false;
}