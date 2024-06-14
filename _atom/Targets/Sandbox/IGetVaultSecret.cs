namespace Atom.Targets.Sandbox;

[TargetDefinition]
public partial interface IGetVaultSecret
{
    [SecretDefinition("test-vault-secret", "Test secret to be retrieved from Azure Vault")]
    string TestVaultSecret => GetParam(() => TestVaultSecret)!;

    Target GetVaultSecret =>
        d => d
            .RequiresParam(Build.Secrets.TestVaultSecret)
            .Executes(() =>
            {
                Logger.LogInformation("Retrieved secret: {TestVaultSecret}", TestVaultSecret);

                return Task.CompletedTask;
            });
}