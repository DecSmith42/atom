namespace Atom.Targets.Test;

[TargetDefinition]
public partial interface ITestSecretProvider
{
    [SecretDefinition("test-secret", "Test Secret")]
    string TestSecret => GetParam(() => TestSecret)!;

    Target TestSecretProvider =>
        d => d
            .WithDescription("Test Vault")
            .RequiresParam(nameof(TestSecret));
}
