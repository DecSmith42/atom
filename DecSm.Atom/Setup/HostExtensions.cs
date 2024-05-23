namespace DecSm.Atom.Setup;

public static class HostExtensions
{
    public static IAtomConfigurator AddAtom<T>(this IHostApplicationBuilder builder, string[] args)
        where T : BuildDefinition
    {
        var configurator = new AtomConfigurator(builder);
        builder.Services.AddSingleton<IAtomConfigurator>(configurator);
        
        builder.Services.TryAddSingleton<IBuildDefinition, T>();
        builder.Services.AddHostedService<AtomService>();
        builder.Services.TryAddSingleton<BuildExecutor>();
        
        builder.Services.TryAddSingleton<WorkflowGenerator>();
        
        builder.Services.TryAddSingleton<IFileSystem, FileSystem>();
        builder.Services.TryAddSingleton<IParamService, ParamService>();
        builder.Services.TryAddSingleton(new RawCommandLineArgs(args));
        
        builder.Services.TryAddSingleton<CommandLineArgs>(services =>
        {
            var raw = services.GetRequiredService<RawCommandLineArgs>();
            
            return CommandLineArgsParser.Parse(raw.Args, services.GetRequiredService<IBuildDefinition>());
        });
        
        builder.Services.TryAddSingleton<BuildModel>(services =>
        {
            var buildDefinition = services.GetRequiredService<IBuildDefinition>();
            var commandLineArgs = services.GetRequiredService<CommandLineArgs>();
            
            return BuildResolver.ResolveBuild(buildDefinition,
                commandLineArgs
                    .Commands
                    .Select(x => x.Name)
                    .ToArray(),
                commandLineArgs.HasSkip);
        });
        
        builder.Services.TryAddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);
        builder.Services.TryAddSingleton<ICheatsheetService, CheatsheetService>();
        
        builder.Logging.ClearProviders();
        
        builder.Logging.AddProvider(new SpectreLoggerProvider());
        
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