namespace DecSm.Atom.Tests.BuildTests.Workflows;

[BuildDefinition]
public partial class WorkflowDependentTargetBuild : BuildDefinition, IWorkflowDependentTarget1, IWorkflowDependentTarget2
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("workflow-2")
        {
            Triggers = [new TestWorkflowTrigger()],
            StepDefinitions = [Commands.WorkflowTarget2],
            Options = [new TestWorkflowOption()],
            WorkflowTypes = [new TestWorkflowType()],
        },
    ];
}

[TargetDefinition]
public partial interface IWorkflowDependentTarget1
{
    Target WorkflowDependentTarget1 =>
        d => d
            .WithDescription("Workflow Target 1")
            .Executes(() => Task.CompletedTask);
}

[TargetDefinition]
public partial interface IWorkflowDependentTarget2
{
    Target WorkflowTarget2 =>
        d => d
            .WithDescription("Workflow Target 2")
            .DependsOn(nameof(IWorkflowDependentTarget1.WorkflowDependentTarget1))
            .Executes(() => Task.CompletedTask);
}
