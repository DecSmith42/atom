namespace DecSm.Atom.Tests.BuildTests.Workflows;

[BuildDefinition]
public partial class WorkflowSingleTargetBuild : BuildDefinition, IWorkflowSingleTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("workflow-1")
        {
            Triggers = [new TestWorkflowTrigger()],
            StepDefinitions = [Commands.WorkflowSingleTarget],
            Options = [new TestWorkflowOption()],
            WorkflowTypes = [new TestWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface IWorkflowSingleTarget
{
    Target WorkflowSingleTarget =>
        d => d
            .WithDescription("Workflow Target 1")
            .Executes(() => Task.CompletedTask);
}
