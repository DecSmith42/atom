namespace DecSm.Atom.Params;

[AttributeUsage(AttributeTargets.Property)]
public class ParamAttribute(string argName, string description, string? defaultValue = null) : Attribute
{
    public string ArgName => argName;
    
    public string Description => description;
    
    public string? DefaultValue => defaultValue;
    
    public bool SourceFromCliArguments { get; init; } = true;
    
    public bool SourceFromEnvironmentVariables { get; init; } = true;
    
    public bool SourceFromConfigurationFiles { get; init; } = true;
    
    public bool SourceFromSecrets { get; protected init; }
}

public class SecretParamAttribute : ParamAttribute
{
    public SecretParamAttribute(string argName, string description, string? defaultValue = null) : base(argName, description, defaultValue)
    {
        SourceFromSecrets = true;
    }
}