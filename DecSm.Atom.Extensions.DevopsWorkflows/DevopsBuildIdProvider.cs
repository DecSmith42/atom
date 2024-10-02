namespace DecSm.Atom.Extensions.DevopsWorkflows;

public sealed class DevopsBuildIdProvider(IBuildDefinition buildDefinition) : IBuildIdProvider
{
    public string? BuildId =>
        ProvideDevopsRunIdAsWorkflowId.IsEnabled(IWorkflowOption.GetOptionsForCurrentTarget(buildDefinition))
            ? Devops.Variables.BuildBuildId
            : null;
}
