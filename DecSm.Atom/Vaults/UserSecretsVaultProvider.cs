namespace DecSm.Atom.Vaults;

public class UserSecretsVaultProvider : IVaultProvider
{
    public Assembly? SecretsAssembly { get; set; }

    public string? GetSecret(string key)
    {
        var userSecrets = new ConfigurationBuilder()
            .AddUserSecrets(SecretsAssembly ?? Assembly.GetEntryAssembly()!)
            .Build()
            .AsEnumerable()
            .ToDictionary(x => x.Key, x => x.Value);

        return userSecrets.GetValueOrDefault(key);
    }
}
