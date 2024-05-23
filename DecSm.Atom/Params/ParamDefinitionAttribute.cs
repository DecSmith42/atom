namespace DecSm.Atom.Params;

[AttributeUsage(AttributeTargets.Property)]
public class ParamDefinitionAttribute(string argName, string description, string? defaultValue = null) : Attribute
{
    public string ArgName => argName;
    
    public string Description => description;
    
    public string? DefaultValue => defaultValue;
    
    public bool IsSecret { get; protected init; }
}

public class SecretDefinitionAttribute : ParamDefinitionAttribute
{
    public SecretDefinitionAttribute(string argName, string description, string? defaultValue = null) : base(argName,
        description,
        defaultValue)
    {
        IsSecret = true;
    }
}