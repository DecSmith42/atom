namespace DecSm.Atom.Module.DevopsWorkflows;

[TargetDefinition]
public partial interface IDevopsWorkflows : IJobRunsOn
{
    static void IBuildDefinition.Register(IServiceCollection services)
    {
        services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter), typeof(DevopsWorkflowWriter), ServiceLifetime.Singleton));

        services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowVariableProvider),
            typeof(DevopsVariableProvider),
            ServiceLifetime.Singleton));

        if (Devops.IsDevopsPipelines)
            services
                .AddSingleton<IOutcomeReporter, DevopsSummaryOutcomeReporter>()
                .ProvidePath((key, locator) => Devops.IsDevopsPipelines
                    ? key switch
                    {
                        AtomPaths.Artifacts => locator(AtomPaths.Root)
                                                   .Parent! /
                                               "a",
                        AtomPaths.Publish => locator(AtomPaths.Root)
                                                 .Parent! /
                                             "b",
                        _ => null,
                    }
                    : null);
    }
}
