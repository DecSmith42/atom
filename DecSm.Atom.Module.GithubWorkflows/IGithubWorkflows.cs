namespace DecSm.Atom.Module.GithubWorkflows;

[ConfigureHostBuilder]
public partial interface IGithubWorkflows : IJobRunsOn
{
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder)
    {
        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(GithubWorkflowWriter),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(DependabotWorkflowWriter),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowVariableProvider),
            typeof(GithubVariableProvider),
            ServiceLifetime.Singleton));

        if (!Github.IsGithubActions)
            return;

        if (Github.Variables.RunnerDebug is "1")
            LogOptions.IsVerboseEnabled = true;

        builder
            .Services
            .AddSingleton<IOutcomeReportWriter, GithubSummaryOutcomeReportWriter>()
            .ProvidePath((key, locator) => Github.IsGithubActions
                ? key switch
                {
                    AtomPaths.Artifacts => locator(AtomPaths.Root) / ".github" / "artifacts",
                    AtomPaths.Publish => locator(AtomPaths.Root) / ".github" / "publish",
                    _ => null,
                }
                : null);
    }
}
