namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SnapshotImageBuild : MinimalBuildDefinition, IGithubWorkflows, ISnapshotImageTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("snapshotimageoption-workflow")
        {
            Triggers = [ManualTrigger.Empty],
            Targets =
            [
                WorkflowTargets.SnapshotImageTarget.WithOptions(
                    GithubSnapshotImageOption.Create(new("snapshot-image-test", "1.*.*"))),
            ],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

public interface ISnapshotImageTarget
{
    Target SnapshotImageTarget => t => t;
}
