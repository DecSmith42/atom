namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class CheckoutOptionBuild : MinimalBuildDefinition, IGithubWorkflows, ICheckoutOptionTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("checkoutoption-workflow")
        {
            Triggers = [ManualTrigger.Empty],
            Targets =
            [
                WorkflowTargets.CheckoutOptionTarget.WithOptions(GithubCheckoutOption.Create(new("v4",
                    true,
                    "recursive",
                    "some-token"))),
            ],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface ICheckoutOptionTarget
{
    Target CheckoutOptionTarget => t => t;
}
