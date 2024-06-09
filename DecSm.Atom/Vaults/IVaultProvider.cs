namespace DecSm.Atom.Vaults;

public interface IVaultProvider
{
    string? GetSecret(string key);
}