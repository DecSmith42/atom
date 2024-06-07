namespace DecSm.Atom.Vault;

public interface IVaultProvider
{
    string? GetSecret(string key);
}