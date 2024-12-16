namespace DecSm.Atom.Secrets;

public class DotnetUserSecretsProvider : ISecretsProvider
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
