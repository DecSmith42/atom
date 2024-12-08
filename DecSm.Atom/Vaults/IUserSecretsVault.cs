namespace DecSm.Atom.Vaults;

[TargetDefinition]
public partial interface IUserSecretsVault
{
    static void IBuildDefinition.Register(IServiceCollection services) =>
        services
            .AddSingleton<UserSecretsVaultProvider>()
            .AddSingleton<IVaultProvider>(x => x.GetRequiredService<UserSecretsVaultProvider>());
}
