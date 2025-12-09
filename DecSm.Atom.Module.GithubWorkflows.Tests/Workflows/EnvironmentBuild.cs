namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class EnvironmentBuild : MinimalBuildDefinition, IGithubWorkflows, IEnvironmentTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("environment-workflow")
        {
            Triggers = [ManualTrigger.Empty],
            Targets = [WorkflowTargets.EnvironmentTarget.WithOptions(DeployToEnvironment.Create("test-env-1"))],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface IEnvironmentTarget
{
    Target EnvironmentTarget => t => t;
}
