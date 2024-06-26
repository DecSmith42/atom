﻿namespace DecSm.Atom.Setup;

public static class HostExtensions
{
    public static IAtomConfiguration AddAtom<T>(this IHostApplicationBuilder builder, string[] args)
        where T : BuildDefinition
    {
        var configurator = new AtomConfiguration(builder);
        builder.Services.AddHostedService<AtomService>();

        builder.Services.AddSingleton<IAtomConfiguration>(configurator);

        builder.Services.AddSingleton<IBuildDefinition, T>();
        builder.Services.AddSingleton<IBuildExecutor, BuildExecutor>();
        builder.Services.AddSingleton<IWorkflowGenerator, WorkflowGenerator>();

        builder.Services.AddSingleton<ParamService>();

        builder.Services.AddSingleton<IParamService>(services =>
            ParamServiceAccessor.Service = services.GetRequiredService<ParamService>());

        builder.Services.TryAddSingleton<IWorkflowVariableProvider, AtomWorkflowVariableProvider>();
        builder.Services.TryAddSingleton<IWorkflowVariableService, WorkflowVariableService>();
        builder.Services.TryAddSingleton<IBuildIdProvider, AtomBuildIdProvider>();
        builder.Services.TryAddSingleton<IBuildVersionProvider, AtomBuildVersionProvider>();

        builder.Services.AddSingleton<CommandLineArgs>(services =>
            CommandLineArgsParser.Parse(args, services.GetRequiredService<IBuildDefinition>()));

        builder.Services.AddSingleton<BuildModel>(services =>
        {
            var buildDefinition = services.GetRequiredService<IBuildDefinition>();
            var commandLineArgs = services.GetRequiredService<CommandLineArgs>();

            return BuildResolver.ResolveBuild(buildDefinition,
                commandLineArgs
                    .Commands
                    .Select(x => x.Name)
                    .ToArray(),
                !commandLineArgs.HasSkip);
        });

        builder.Services.AddSingleton<IFileSystem, FileSystem>();
        builder.Services.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);
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