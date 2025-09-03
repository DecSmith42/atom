namespace DecSm.Atom.Module.LocalWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SimpleBuild : BuildDefinition, IPowershellWorkflows, ISimpleTarget
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
            WorkflowTypes = [new PowershellWorkflowType()],
        },
    ];
}

public interface ISimpleTarget
{
    Target SimpleTarget => t => t;
}
