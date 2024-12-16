namespace DecSm.Atom.Secrets;

[TargetDefinition]
public partial interface IDotnetUserSecrets
{
    static void IBuildDefinition.Register(IServiceCollection services) =>
        services
            .AddSingleton<DotnetUserSecretsProvider>()
            .AddSingleton<ISecretsProvider>(x => x.GetRequiredService<DotnetUserSecretsProvider>());
}
