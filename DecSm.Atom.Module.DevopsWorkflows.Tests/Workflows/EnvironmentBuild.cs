namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[MinimalBuildDefinition]
public partial class EnvironmentBuild : MinimalBuildDefinition, IDevopsWorkflows, IEnvironmentTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("environment-workflow")
        {
            Triggers = [ManualTrigger.Empty],
            Targets = [Targets.EnvironmentTarget.WithOptions(DeployToEnvironment.Create("test-env-1"))],
            WorkflowTypes = [Devops.WorkflowType],
        },
    ];
}

[TargetDefinition]
public partial interface IEnvironmentTarget
{
    Target EnvironmentTarget => t => t;
}
