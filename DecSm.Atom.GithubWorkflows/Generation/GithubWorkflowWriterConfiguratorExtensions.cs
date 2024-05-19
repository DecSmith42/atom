namespace DecSm.Atom.GithubWorkflows.Generation;

public static class GithubWorkflowWriterConfiguratorExtensions
{
    public static IAtomConfigurator AddGithubWorkflows(this IAtomConfigurator configurator)
    {
        configurator.Builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IAtomWorkflowWriter),
            typeof(GithubWorkflowWriter),
            ServiceLifetime.Singleton));
        
        configurator.Builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IAtomWorkflowWriter),
            typeof(DependabotWorkflowWriter),
            ServiceLifetime.Singleton));
        
        return configurator;
    }
}