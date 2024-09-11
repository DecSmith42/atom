namespace DecSm.Atom.Hosting;

public static class HostExtensions
{
    public static TBuilder AddAtom<TBuilder, TBuild>(this TBuilder builder, string[] args)
        where TBuilder : IHostApplicationBuilder
        where TBuild : BuildDefinition, IBuildDefinition
    {
        builder.Services.AddHostedService<AtomService>();
        builder.Services.AddSingleton<IBuildDefinition, TBuild>();

        TBuild.Register(builder.Services);

        builder.Services.AddAccessedSingleton<IParamService, ParamService>();
        builder.Services.AddAccessedSingleton<IReportService, ReportService>();

        builder
            .Services
            .AddSingleton<IAtomFileSystem>(x => new AtomFileSystem
            {
                FileSystem = new FileSystem(),
                PathLocators = x
                    .GetServices<IPathProvider>()
                    .OrderByDescending(l => l.Priority)
                    .ToList(),
            })
            .AddSingleton<IFileSystem>(x => x.GetRequiredService<IAtomFileSystem>());

        builder.Services.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);
        builder.Services.AddSingleton<IBuildExecutor, BuildExecutor>();
        builder.Services.AddSingleton<IWorkflowGenerator, WorkflowGenerator>();
        builder.Services.AddSingleton<IProcessRunner, ProcessRunner>();
        builder.Services.AddSingleton<IOutcomeReporter, ConsoleOutcomeReporter>();
        builder.Services.AddSingleton<IWorkflowVariableProvider, AtomWorkflowVariableProvider>();

        builder.Services.TryAddSingleton<IWorkflowVariableService, WorkflowVariableService>();
        builder.Services.TryAddSingleton<IBuildIdProvider, AtomBuildIdProvider>();
        builder.Services.TryAddSingleton<IBuildVersionProvider, AtomBuildVersionProvider>();
        builder.Services.TryAddSingleton<ICheatsheetService, CheatsheetService>();

        builder.Services.AddSingleton<CommandLineArgsParser>();

        builder.Services.AddSingleton<CommandLineArgs>(services =>
        {
            var parsedArgs = services
                .GetRequiredService<CommandLineArgsParser>()
                .Parse(args);

            if (parsedArgs.HasVerbose)
                LogOptions.IsVerboseEnabled = true;

            return parsedArgs;
        });

        builder.Services.AddSingleton<BuildResolver>();
        builder.Services.AddSingleton<WorkflowResolver>();

        builder.Services.AddSingleton<BuildModel>(services => services
            .GetRequiredService<BuildResolver>()
            .Resolve());

        builder.Logging.ClearProviders();

        builder.Logging.AddProvider(new SpectreLoggerProvider());
        builder.Logging.AddProvider(new ReportLoggerProvider());

        builder.Logging.AddFilter((context, level) => (context, level) switch
        {
            ("Microsoft.Hosting.Lifetime", < LogLevel.Warning) => false,
            ("Microsoft.Extensions.Hosting", < LogLevel.Warning) => false,
            ("Microsoft.Extensions.Hosting.Internal.Host", < LogLevel.Warning) => false,
            _ => true,
        });

        return builder;
    }
}
