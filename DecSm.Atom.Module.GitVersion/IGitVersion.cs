namespace DecSm.Atom.Module.GitVersion;

[ConfigureHostBuilder]
public partial interface IGitVersion
{
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<IBuildIdProvider, GitVersionBuildIdProvider>()
            .AddSingleton<IBuildVersionProvider, GitVersionBuildVersionProvider>();
}
