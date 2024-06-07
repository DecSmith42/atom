using DecSm.Atom.Setup;
using DecSm.Atom.Variables;
using DecSm.Atom.Workflows.Writer;

namespace DecSm.Atom.GithubWorkflows;

public static class GithubWorkflowWriterConfiguratorExtensions
{
    public static IAtomConfiguration AddGithubWorkflows(this IAtomConfiguration configuration)
    {
        configuration.Builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IAtomWorkflowWriter),
            typeof(GithubWorkflowWriter),
            ServiceLifetime.Singleton));
        
        configuration.Builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IAtomWorkflowWriter),
            typeof(DependabotWorkflowWriter),
            ServiceLifetime.Singleton));
        
        configuration.Builder.Services.TryAdd(new ServiceDescriptor(typeof(IWorkflowVariableProvider),
            typeof(GithubVariableProvider),
            ServiceLifetime.Singleton));
        
        return configuration;
    }
}