﻿namespace DecSm.Atom.Module.LocalWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class DependentBuild : BuildDefinition, IPowershellWorkflows, IDependentTarget1, IDependentTarget2
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
            WorkflowTypes = [new PowershellWorkflowType()],
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
