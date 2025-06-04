namespace Atom.Targets.Test;

[TargetDefinition]
public partial interface ITestSecretProvider
{
    [SecretDefinition("test-secret", "Test Secret")]
    string TestSecret => GetParam(() => TestSecret)!;

    Target TestSecretProvider =>
        t => t
            .DescribedAs("Test Vault")
            .RequiresParam(nameof(TestSecret));
}
