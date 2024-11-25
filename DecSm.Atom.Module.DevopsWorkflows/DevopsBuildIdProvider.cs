namespace DecSm.Atom.Module.DevopsWorkflows;

internal sealed class DevopsBuildIdProvider(IBuildDefinition buildDefinition) : IBuildIdProvider
{
    public string? BuildId =>
        ProvideDevopsRunIdAsWorkflowId.IsEnabled(IWorkflowOption.GetOptionsForCurrentTarget(buildDefinition))
            ? Devops.Variables.BuildBuildId
            : null;
}
