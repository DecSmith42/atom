[BuildDefinition]
public partial class Build : IPackAtom
{
    public override WorkflowDefinition[] Workflows =>
    [
        new("Build", [new CommandWorkflowStepDefinition(nameof(IPackAtom.PackAtom))], [new BatWorkflowType()]),
    ];
}