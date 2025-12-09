#pragma warning disable CA1822

namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class GithubIfBuild : MinimalBuildDefinition, IGithubWorkflows
{
    private Target GithubIfTarget => t => t;

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("githubif-workflow")
        {
            Triggers = [ManualTrigger.Empty],
            Targets =
            [
                WorkflowTargets.GithubIfTarget.WithOptions(
                    GithubIf.Create(new GreaterThanExpression(new NumberExpression(4), new NumberExpression(3)))),
            ],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}
