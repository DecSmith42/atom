namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[MinimalBuildDefinition]
public partial class SetupDotnetBuild : MinimalBuildDefinition, IDevopsWorkflows, ISetupDotnetTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("setup-dotnet")
        {
            Triggers = [GitPushTrigger.ToMain],
            Targets = [Targets.SetupDotnetTarget.WithOptions(new SetupDotnetStep("9.0.x"))],
            WorkflowTypes = [Devops.WorkflowType],
        },
    ];
}

[TargetDefinition]
public partial interface ISetupDotnetTarget
{
    Target SetupDotnetTarget => t => t;
}
