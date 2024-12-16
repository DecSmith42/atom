namespace DecSm.Atom.Secrets;

[PublicAPI]
public interface ISecretsProvider
{
    string? GetSecret(string key);
}
