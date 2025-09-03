namespace DecSm.Atom.Module.LocalWorkflows;

[ConfigureHostBuilder]
public partial interface IPowershellWorkflows : IJobRunsOn
{
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IWorkflowWriter),
            typeof(PowershellWorkflowWriter),
            ServiceLifetime.Singleton));
}
