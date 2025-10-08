namespace DecSm.Atom.Params;

/// <summary>
///     Attribute to define a secret parameter for a command.
/// </summary>
/// <remarks>If attributing a non-secret value, use <see cref="ParamDefinitionAttribute" /> instead.</remarks>
[PublicAPI]
public class SecretDefinitionAttribute : ParamDefinitionAttribute
{
    public SecretDefinitionAttribute(
        string argName,
        string description,
        ParamSource sources = ParamSource.All,
        string[]? chainedParams = null) : base(argName, description, sources, chainedParams)
    {
        IsSecret = true;
    }
}
