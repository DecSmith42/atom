namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class DependabotBuild : MinimalBuildDefinition, IGithubWorkflows
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        Github.DependabotWorkflow(new()
        {
            Registries =
            [
                new("registry-1", "type1", "url1")
                {
                    Username = new LiteralExpression("username1"),
                    Password = new LiteralExpression("secrets.PASSWORD").Expression,
                    Token = new LiteralExpression("secrets.TOKEN").Expression,
                },
                new("registry-2", "type2", "url2"),
            ],
            Updates =
            [
                new("update-1", "package-ecosystem-1", "directory-1", 1, DependabotSchedule.Daily)
                {
                    Registries = ["registry-1", "registry-2"],
                    Groups =
                    [
                        new("group-1")
                        {
                            Patterns = ["pattern-1", "pattern-2"],
                        },
                    ],
                },
                new("update-2", "package-ecosystem-2", "directory-2", 2, DependabotSchedule.Monthly),
            ],
        }),
    ];
}
