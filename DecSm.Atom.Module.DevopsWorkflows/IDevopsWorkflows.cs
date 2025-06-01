namespace DecSm.Atom.Module.DevopsWorkflows;

[ConfigureBuilder]
public partial interface IDevopsWorkflows : IJobRunsOn
{
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder)
    {
        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(DevopsWorkflowWriter),
            ServiceLifetime.Singleton));

        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowVariableProvider),
            typeof(DevopsVariableProvider),
            ServiceLifetime.Singleton));

        if (Devops.IsDevopsPipelines)
            builder
                .Services
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
