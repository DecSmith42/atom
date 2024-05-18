[BuildDefinition]
public partial class Build : IPackAtom
{
    public override WorkflowDefinition[] Workflows =>
    [
        new("Build")
        {
            StepDefinitions = [new CommandWorkflowStepDefinition(nameof(IPackAtom.PackAtom))],
            WorkflowTypes = [new BatWorkflowType(), new GithubWorkflowType()],
        },
    ];
}