namespace DecSm.Atom.GithubWorkflows;

public static class GithubWorkflowWriterConfiguratorExtensions
{
    public static IAtomConfigurator AddGithub(this IAtomConfigurator configurator)
    {
        configurator.Builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IAtomWorkflowWriter),
            typeof(GithubWorkflowWriter),
            ServiceLifetime.Singleton));
        
        configurator.Builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IAtomWorkflowWriter),
            typeof(DependabotWorkflowWriter),
            ServiceLifetime.Singleton));
        
        configurator.Builder.Services.TryAdd(new ServiceDescriptor(typeof(IWorkflowVariableProvider),
            typeof(GithubVariableProvider),
            ServiceLifetime.Singleton));
        
        return configurator;
    }
}