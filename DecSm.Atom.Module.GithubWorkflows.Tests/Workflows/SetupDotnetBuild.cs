﻿namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SetupDotnetBuild : BuildDefinition, IGithubWorkflows, ISetupDotnetTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("setup-dotnet")
        {
            Triggers = [GitPushTrigger.ToMain],
            Targets = [Targets.SetupDotnetTarget.WithOptions(new SetupDotnetStep("9.0.x"))],
            WorkflowTypes = [Github.WorkflowType],
        },
    ];
}

[TargetDefinition]
public partial interface ISetupDotnetTarget
{
    Target SetupDotnetTarget => t => t;
}
