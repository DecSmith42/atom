namespace DecSm.Atom.Tests.BuildTests.Vaults;

[BuildDefinition]
public class UserSecretsBuild : BuildDefinition, IUserSecretsTarget, IUserSecretsVault
{
    public string? ExecutionValue { get; set; }
}

[TargetDefinition]
public interface IUserSecretsTarget
{
    [SecretDefinition("secret-1", "Secret 1")]
    string? Secret1 => GetParam(() => Secret1);

    string? ExecutionValue { get; set; }

    Target UserSecretsTarget =>
        d => d
            .WithDescription("User secrets target")
            .RequiresParam(nameof(Secret1))
            .Executes(() =>
            {
                ExecutionValue = Secret1;

                return Task.CompletedTask;
            });
}
