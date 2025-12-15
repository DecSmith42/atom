namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ReleaseTriggerBuild : MinimalBuildDefinition, IGithubWorkflows, IReleaseTriggerTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("releasetrigger-workflow")
        {
            Triggers = [GithubReleaseTrigger.OnReleased],
            Targets = [WorkflowTargets.ReleaseTriggerTarget],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

public interface IReleaseTriggerTarget
{
    Target ReleaseTriggerTarget => t => t;
}
