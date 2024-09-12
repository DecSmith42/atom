namespace DecSm.Atom.Extensions.GithubWorkflows;

[TargetDefinition]
public partial interface IGithubWorkflows
{
    [ParamDefinition("github-runs-on", "Github runner to use for a job")]
    string GithubRunsOn => GetParam(() => GithubRunsOn)!;

    static void IBuildDefinition.Register(IServiceCollection services)
    {
        services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter), typeof(GithubWorkflowWriter), ServiceLifetime.Singleton));

        services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(DependabotWorkflowWriter),
            ServiceLifetime.Singleton));

        services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowVariableProvider),
            typeof(GithubVariableProvider),
            ServiceLifetime.Singleton));

        services.AddSingleton<IBuildIdProvider, GithubBuildIdProvider>();

        if (Github.IsGithubActions)
            services
                .AddSingleton<IBuildIdProvider, GithubBuildIdProvider>()
                .AddSingleton<IOutcomeReporter, GithubSummaryOutcomeReporter>()
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
