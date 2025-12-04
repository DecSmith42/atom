namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[MinimalBuildDefinition]
public partial class CheckoutOptionBuild : MinimalBuildDefinition, IDevopsWorkflows, ICheckoutOptionTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("checkoutoption-workflow")
        {
            Triggers = [ManualTrigger.Empty],
            Targets = [Targets.CheckoutOptionTarget.WithOptions(DevopsCheckoutOption.Create(new(true, "recursive")))],
            WorkflowTypes = [Devops.WorkflowType],
        },
    ];
}

[TargetDefinition]
public partial interface ICheckoutOptionTarget
{
    Target CheckoutOptionTarget => t => t;
}
