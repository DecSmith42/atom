namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[MinimalBuildDefinition]
public partial class SimpleBuild : MinimalBuildDefinition, IGithubWorkflows, ISimpleTarget
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
            Targets = [Targets.SimpleTarget],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface ISimpleTarget
{
    Target SimpleTarget => t => t;
}
