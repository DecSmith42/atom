namespace DecSm.Atom.Workflows.Implementation.Workflow.Bat;

public static class BatWorkflowWriterConfiguratorExtensions
{
    public static IAtomConfigurator AddBatWorkflows(this IAtomConfigurator configurator)
    {
        configurator.Builder.Services.TryAddEnumerable(
            new ServiceDescriptor(typeof(IAtomWorkflowWriter),
                typeof(BatWorkflowWriter),
                ServiceLifetime.Singleton));

        return configurator;
    }
}