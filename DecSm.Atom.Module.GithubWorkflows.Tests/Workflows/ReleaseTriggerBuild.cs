namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ReleaseTriggerBuild : BuildDefinition, IGithubWorkflows, IReleaseTriggerTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("releasetrigger-workflow")
        {
            Triggers = [GithubReleaseTrigger.OnReleased],
            StepDefinitions = [Commands.ReleaseTriggerTarget],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface IReleaseTriggerTarget
{
    Target ReleaseTriggerTarget => d => d;
}
