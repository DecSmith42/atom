namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SimpleBuild : BuildDefinition, IGithubWorkflows, ISimpleTarget
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
            StepDefinitions = [Commands.SimpleTarget],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface ISimpleTarget
{
    Target SimpleTarget => d => d;
}
