namespace DecSm.Atom.Module.LocalWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class EnvironmentBuild : BuildDefinition, IPowershellWorkflows, IEnvironmentTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("environment-workflow")
        {
            Triggers = [ManualTrigger.Empty],
            Targets = [Targets.EnvironmentTarget.WithOptions(DeployToEnvironment.Create("test-env-1"))],
            WorkflowTypes = [new PowershellWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface IEnvironmentTarget
{
    Target EnvironmentTarget => t => t;
}
