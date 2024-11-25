namespace DecSm.Atom.Params;

[PublicAPI]
public class SecretDefinitionAttribute : ParamDefinitionAttribute
{
    public SecretDefinitionAttribute(string argName, string description, string? defaultValue = null) : base(argName,
        description,
        defaultValue)
    {
        IsSecret = true;
    }
}
