namespace Atom.Targets.Test;

[TargetDefinition]
public partial interface ITestVault
{
    [SecretDefinition("test-secret", "Test Secret")]
    string TestSecret => GetParam(() => TestSecret)!;

    Target TestVault =>
        d => d
            .WithDescription("Test Vault")
            .RequiresParam(nameof(TestSecret));
}
