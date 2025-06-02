namespace DecSm.Atom.Tests.BuildTests.Secrets;

[BuildDefinition]
public partial class UserSecretsBuild : BuildDefinition, IUserSecretsTarget, IDotnetUserSecrets
{
    public string? ExecutionValue { get; set; }
}

[TargetDefinition]
public partial interface IUserSecretsTarget
{
    [SecretDefinition("secret-1", "Secret 1")]
    string? Secret1 => GetParam(() => Secret1);

    string? ExecutionValue { get; set; }

    Target UserSecretsTarget =>
        d => d
            .DescribedAs("User secrets target")
            .RequiresParam(nameof(Secret1))
            .Executes(() =>
            {
                ExecutionValue = Secret1;

                return Task.CompletedTask;
            });
}
