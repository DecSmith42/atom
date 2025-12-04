namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[MinimalBuildDefinition]
public partial class DependentBuild : MinimalBuildDefinition, IDevopsWorkflows, IDependentTarget1, IDependentTarget2
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("dependent-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets = [Targets.DependentTarget1, Targets.DependentTarget2],
            WorkflowTypes = [Devops.WorkflowType],
        },
    ];
}

[TargetDefinition]
public partial interface IDependentTarget1
{
    Target DependentTarget1 => t => t;
}

[TargetDefinition]
public partial interface IDependentTarget2
{
    Target DependentTarget2 => t => t.DependsOn(nameof(IDependentTarget1.DependentTarget1));
}
