namespace Atom.Targets.Test;

[TargetDefinition]
public partial interface ITestSecretProvider
{
    [SecretDefinition("test-secret", "Test Secret")]
    string TestSecret => GetParam(() => TestSecret)!;

    Target TestSecretProvider =>
        d => d
            .DescribedAs("Test Vault")
            .RequiresParam(nameof(TestSecret));
}
