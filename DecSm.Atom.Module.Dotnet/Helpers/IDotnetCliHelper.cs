namespace DecSm.Atom.Module.Dotnet.Helpers;

[PublicAPI]
[ConfigureHostBuilder]
public partial interface IDotnetCliHelper : IBuildAccessor
{
    IDotnetCli DotnetCli => GetService<IDotnetCli>();

    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IDotnetCli, DotnetCli>();
}
