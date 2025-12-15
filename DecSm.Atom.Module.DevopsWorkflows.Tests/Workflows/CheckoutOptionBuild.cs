namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class CheckoutOptionBuild : MinimalBuildDefinition, IDevopsWorkflows, ICheckoutOptionTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("checkoutoption-workflow")
        {
            Triggers = [ManualTrigger.Empty],
            Targets =
            [
                WorkflowTargets.CheckoutOptionTarget.WithOptions(
                    DevopsCheckoutOption.Create(new(true, "recursive"))),
            ],
            WorkflowTypes = [Devops.WorkflowType],
        },
    ];
}

public interface ICheckoutOptionTarget
{
    Target CheckoutOptionTarget => t => t;
}
