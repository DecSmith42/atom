namespace DecSm.Atom.Secrets;

[ConfigureBuilder]
public partial interface IDotnetUserSecrets
{
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<DotnetUserSecretsProvider>()
            .AddSingleton<ISecretsProvider>(x => x.GetRequiredService<DotnetUserSecretsProvider>());
}
