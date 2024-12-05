namespace DecSm.Atom.Params;

[PublicAPI]
public class SecretDefinitionAttribute : ParamDefinitionAttribute
{
    public SecretDefinitionAttribute(string argName, string description, string? defaultValue = null, ParamSource sources = ParamSource.All)
        : base(argName, description, defaultValue, sources)
    {
        IsSecret = true;
    }
}
