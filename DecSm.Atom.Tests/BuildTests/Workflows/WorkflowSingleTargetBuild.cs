namespace DecSm.Atom.Tests.BuildTests.Workflows;

[MinimalBuildDefinition]
public partial class WorkflowSingleTargetBuild : IWorkflowSingleTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("workflow-1")
        {
            Triggers = [new TestWorkflowTrigger()],
            Targets = [Targets.WorkflowSingleTarget],
            Options = [new TestWorkflowOption()],
            WorkflowTypes = [new TestWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface IWorkflowSingleTarget
{
    Target WorkflowSingleTarget =>
        t => t
            .DescribedAs("Workflow Target 1")
            .Executes(() => Task.CompletedTask);
}
