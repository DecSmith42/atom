namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class EnvironmentBuild : MinimalBuildDefinition, IDevopsWorkflows, IEnvironmentTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("environment-workflow")
        {
            Triggers = [ManualTrigger.Empty],
            Targets = [WorkflowTargets.EnvironmentTarget.WithOptions(DeployToEnvironment.Create("test-env-1"))],
            WorkflowTypes = [Devops.WorkflowType],
        },
    ];
}

public interface IEnvironmentTarget
{
    Target EnvironmentTarget => t => t;
}
