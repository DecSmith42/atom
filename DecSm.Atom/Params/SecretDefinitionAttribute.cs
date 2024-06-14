namespace DecSm.Atom.Params;

public class SecretDefinitionAttribute : ParamDefinitionAttribute
{
    public SecretDefinitionAttribute(string argName, string description, string? defaultValue = null) : base(argName,
        description,
        defaultValue)
    {
        IsSecret = true;
    }
}