namespace DecSm.Atom.Sample;

[BuildDefinition]
public partial class Build : ITest1, ITest2
{
    public override WorkflowDefinition[] Workflows =>
    [
        new("TestWorkflow1",
            [
                new CommandWorkflowStepDefinition(nameof(ITest1.Test1)),
                new CommandWorkflowStepDefinition(nameof(ITest2.Test2)),
            ],
            [
                new(),
            ]),
    ];
}