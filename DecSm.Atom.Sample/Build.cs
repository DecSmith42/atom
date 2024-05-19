namespace DecSm.Atom.Sample;

[BuildDefinition]
public partial class Build : ITest1, ITestDependency
{
    public override WorkflowDefinition[] Workflows =>
    [
        new("TestWorkflow1")
        {
            StepDefinitions =
            [
                new CommandDefinition(nameof(ITest1.Test1)), new CommandDefinition(nameof(ITestDependency.TestDependency)),
            ],
            WorkflowTypes = [new BatWorkflowType()],
        },
    ];
}