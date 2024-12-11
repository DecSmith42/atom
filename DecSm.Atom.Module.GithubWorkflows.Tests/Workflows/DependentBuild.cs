namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class DependentBuild : BuildDefinition, IGithubWorkflows, IDependentTarget1, IDependentTarget2
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("dependent-workflow")
        {
            Triggers =
            [
                new GithubPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            StepDefinitions = [Commands.DependentTarget1, Commands.DependentTarget2],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface IDependentTarget1
{
    Target DependentTarget1 => d => d;
}

[TargetDefinition]
public partial interface IDependentTarget2
{
    Target DependentTarget2 => d => d.DependsOn(nameof(IDependentTarget1.DependentTarget1));
}
