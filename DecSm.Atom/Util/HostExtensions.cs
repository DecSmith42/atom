namespace DecSm.Atom.Util;

public static class HostExtensions
{
    public static IAtomConfigurator AddAtom<T>(this IHostApplicationBuilder builder, string[] args)
        where T : AtomBuildDefinition
    {
        var configurator = new AtomConfigurator(builder);
        builder.Services.AddSingleton<IAtomConfigurator>(configurator);
        
        builder.Services.TryAddSingleton<IAtomBuildDefinition, T>();
        builder.Services.AddHostedService<AtomBuildService>();
        builder.Services.TryAddSingleton<AtomBuildExecutor>();
        builder.Services.TryAddSingleton<ExecutableBuild>();

        builder.Services.TryAddSingleton<AtomWorkflowGenerator>();
        builder.Services.TryAddSingleton<AtomWorkflowBuilder>();

        builder.Services.TryAddSingleton<IFileSystem, FileSystem>();
        builder.Services.TryAddSingleton<IParamService, ParamService>();
        builder.Services.TryAddSingleton(new RawCommandLineArgs(args));
        builder.Services.TryAddSingleton(services =>
        {
            var raw = services.GetRequiredService<RawCommandLineArgs>();
            
            return CommandLineArgsParser.Parse(raw.Args, services.GetRequiredService<IAtomBuildDefinition>());
        });

        builder.Services.TryAddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);
        builder.Services.TryAddSingleton<ICheatsheetService, CheatsheetService>();

        builder.Logging.ClearProviders();
        builder.Logging.AddSpectreConsole();
        builder.Logging.AddFilter((context, level) => (context, level) switch
        {
            ("Microsoft.Hosting.Lifetime", < LogLevel.Warning) => false,
            ("Microsoft.Extensions.Hosting", < LogLevel.Warning) => false,
            ("Microsoft.Extensions.Hosting.Internal.Host", < LogLevel.Warning) => false,
            _ => true,
        });

        return configurator;
    }
}