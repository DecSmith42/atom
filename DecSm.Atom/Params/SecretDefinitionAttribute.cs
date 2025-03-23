namespace DecSm.Atom.Params;

/// <summary>
///     Attribute to define a secret parameter for a command.
/// </summary>
/// <remarks>If attributing a non-secret value, use <see cref="ParamDefinitionAttribute" /> instead.</remarks>
[PublicAPI]
public class SecretDefinitionAttribute : ParamDefinitionAttribute
{
    public SecretDefinitionAttribute(string argName, string description, string? defaultValue = null, ParamSource sources = ParamSource.All)
        : base(argName, description, defaultValue, sources)
    {
        IsSecret = true;
    }
}
