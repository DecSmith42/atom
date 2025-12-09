namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SimpleBuild : MinimalBuildDefinition, IDevopsWorkflows, ISimpleTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("simple-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets = [WorkflowTargets.SimpleTarget],
            WorkflowTypes = [Devops.WorkflowType],
        },
    ];
}

[TargetDefinition]
public partial interface ISimpleTarget
{
    Target SimpleTarget => t => t;
}
