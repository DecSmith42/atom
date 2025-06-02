using DefaultBuildVersionProvider = DecSm.Atom.BuildInfo.DefaultBuildVersionProvider;

namespace DecSm.Atom.Hosting;

/// <summary>
///     Provides extension methods to configure Atom services and dependencies on <see cref="IHostApplicationBuilder" />.
/// </summary>
/// <remarks>
///     This class simplifies the integration of Atom functionality into host applications, ensuring consistent configuration
///     of required services, logging providers, dependency injections, and system defaults for Atom-based hosts.
/// </remarks>
/// <example>
///     Example usage:
///     <code>
/// builder.AddAtom&lt;HostApplicationBuilder, MyBuildDefinition&gt;(args);
/// </code>
/// </example>
[PublicAPI]
public static class HostExtensions
{
    /// <summary>
    ///     Configures services, dependencies, logging, and default settings required by the Atom application framework.
    /// </summary>
    /// <typeparam name="TBuilder">
    ///     Type of the host application builder to configure.
    /// </typeparam>
    /// <typeparam name="TBuild">
    ///     Type of the build definition, which must implement <see cref="IBuildDefinition" />.
    ///     Provides custom registrations and overrides specific to the build.
    /// </typeparam>
    /// <param name="builder">
    ///     The host application builder instance to configure.
    /// </param>
    /// <param name="args">
    ///     Command-line arguments provided to the application.
    /// </param>
    /// <returns>
    ///     The configured host application builder instance.
    /// </returns>
    public static TBuilder AddAtom<TBuilder, TBuild>(this TBuilder builder, string[] args)
        where TBuilder : IHostApplicationBuilder
        where TBuild : BuildDefinition
    {
        builder.Services.AddHostedService<AtomService>();
        builder.Services.AddSingleton<BuildDefinition, TBuild>();
        builder.Services.AddSingleton<IBuildDefinition>(x => x.GetRequiredService<BuildDefinition>());

        // ReSharper disable once SuspiciousTypeConversion.Global - Checked before casting
        if (typeof(TBuild).IsAssignableTo(typeof(IConfigureHost)))
            builder.Services.AddSingleton<IConfigureHost>(x => (IConfigureHost)x.GetRequiredService<BuildDefinition>());

        builder.Services.AddSingletonWithStaticAccessor<IParamService, ParamService>();
        builder.Services.AddSingletonWithStaticAccessor<ReportService>();
        builder.Services.AddSingletonWithStaticAccessor<IAnsiConsole>((_, _) => AnsiConsole.Console);

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IBuildIdProvider, DefaultBuildIdProvider>();
        builder.Services.AddSingleton<IBuildVersionProvider, DefaultBuildVersionProvider>();
        builder.Services.AddSingleton<IBuildTimestampProvider, DefaultBuildTimestampProvider>();

        builder
            .Services
            .AddKeyedSingleton<IFileSystem>("RootFileSystem", new FileSystem())
            .AddSingletonWithStaticAccessor<IAtomFileSystem>((x, _) => new AtomFileSystem(x.GetRequiredService<ILogger<AtomFileSystem>>())
            {
                FileSystem = x.GetRequiredKeyedService<IFileSystem>("RootFileSystem"),
                PathLocators = x
                    .GetServices<IPathProvider>()
                    .OrderByDescending(l => l.Priority)
                    .ToList(),
                ProjectName = x.GetRequiredService<CommandLineArgs>()
                    .ProjectName is { Length: > 0 } p
                    ? p
                    : Assembly.GetEntryAssembly()!.GetName()
                        .Name!,
            })
            .AddSingletonWithStaticAccessor<IFileSystem>((x, _) => x.GetRequiredService<IAtomFileSystem>());

        builder.Services.AddSingleton<BuildExecutor>();
        builder.Services.AddSingleton<WorkflowGenerator>();
        builder.Services.AddSingleton<ProcessRunner>();
        builder.Services.AddSingleton<IOutcomeReportWriter, ConsoleOutcomeReportWriter>();
        builder.Services.AddSingleton<IWorkflowVariableProvider, AtomWorkflowVariableProvider>();

        builder.Services.TryAddSingleton<IWorkflowVariableService, WorkflowVariableService>();
        builder.Services.TryAddSingleton<IBuildTimestampProvider, DefaultBuildTimestampProvider>();
        builder.Services.TryAddSingleton<IBuildVersionProvider, DefaultBuildVersionProvider>();
        builder.Services.TryAddSingleton<CheatsheetService>();

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

        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddProvider(new SpectreLoggerProvider());
        builder.Logging.AddProvider(new ReportLoggerProvider());

        builder.Logging.AddFilter((context, level) => (context, level) switch
        {
            ("Microsoft.Hosting.Lifetime", < LogLevel.Warning) => false,
            ("Microsoft.Extensions.Hosting", < LogLevel.Warning) => false,
            ("Microsoft.Extensions.Hosting.Internal.Host", < LogLevel.Warning) => false,
            _ => true,
        });

        using var tempBuild = builder.Services.BuildServiceProvider();

        tempBuild
            .GetService<IConfigureHost>()
            ?.ConfigureBuildHostBuilder(builder);

        return builder;
    }

    public static IHost UseAtom(this IHost host)
    {
        host
            .Services
            .GetService<IConfigureHost>()
            ?.ConfigureBuildHost(host);

        return host;
    }
}
