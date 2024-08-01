namespace DecSm.Atom.Extensions.GithubWorkflows;

public static class GithubHostExtensions
{
    public static IAtomConfiguration AddGithubWorkflows(this IAtomConfiguration configuration)
    {
        configuration.Builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(GithubWorkflowWriter),
            ServiceLifetime.Singleton));

        configuration.Builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(DependabotWorkflowWriter),
            ServiceLifetime.Singleton));

        configuration.Builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowVariableProvider),
            typeof(GithubVariableProvider),
            ServiceLifetime.Singleton));

        if (Github.IsGithubActions)
            configuration
                .Builder
                .Services
                .AddSingleton<IBuildIdProvider, GithubBuildIdProvider>()
                .AddSingleton<IOutcomeReporter, GithubSummaryOutcomeReporter>();

        return configuration;
    }
}