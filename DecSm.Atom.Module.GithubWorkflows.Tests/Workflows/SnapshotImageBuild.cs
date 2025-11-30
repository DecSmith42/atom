namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SnapshotImageBuild : BuildDefinition, IGithubWorkflows, ICheckoutOptionTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("snapshotimageoption-workflow")
        {
            Triggers = [ManualTrigger.Empty],
            Targets =
            [
                Targets.CheckoutOptionTarget.WithOptions(
                    GithubSnapshotImageOption.Create(new("snapshot-image-test", "1.*.*"))),
            ],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface ISnapshotImageTarget
{
    Target SnapshotImageTarget => t => t;
}
