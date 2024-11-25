namespace DecSm.Atom.Vaults;

[PublicAPI]
public interface IVaultProvider
{
    string? GetSecret(string key);
}
