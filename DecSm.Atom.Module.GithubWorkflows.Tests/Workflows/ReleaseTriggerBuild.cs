namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[MinimalBuildDefinition]
public partial class ReleaseTriggerBuild : MinimalBuildDefinition, IGithubWorkflows, IReleaseTriggerTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("releasetrigger-workflow")
        {
            Triggers = [GithubReleaseTrigger.OnReleased],
            Targets = [Targets.ReleaseTriggerTarget],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface IReleaseTriggerTarget
{
    Target ReleaseTriggerTarget => t => t;
}
