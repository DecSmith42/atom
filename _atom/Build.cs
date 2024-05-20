namespace Atom;

[BuildDefinition]
internal partial class Build : IPackAtom,
    IPackAtomGithubWorkflows,
    IPackAtomSourceGenerators,
    IPushToNuget,
    IDiagnostics,
    IPackAtomTool,
    IInputValue
{
    public override WorkflowDefinition[] Workflows =>
    [
        Github.DependabotWorkflow(new()
        {
            Registries = [new("nuget", DependabotValues.NugetType, DependabotValues.NugetUrl)],
            Updates =
            [
                new(DependabotValues.NugetEcosystem)
                {
                    Registries = ["nuget"],
                    Groups =
                    [
                        new("nuget-deps")
                        {
                            Patterns = ["*"],
                        },
                    ],
                },
            ],
        }),
        new("Validate")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PushToMain],
            StepDefinitions =
            [
                Commands.PackAtom, Commands.PackAtomTool, Commands.PackAtomGithubWorkflows, Commands.PackAtomSourceGenerators,
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        new("Build")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PullIntoMain],
            Options = [new InjectGithubSecret(Secrets.NugetApiKey)],
            StepDefinitions =
            [
                Commands.PackAtom,
                Commands.PackAtomTool,
                Commands.PackAtomGithubWorkflows,
                Commands.PackAtomSourceGenerators,
                Commands.PushToNuget,
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        new("Sandbox")
        {
            Triggers = [Github.Triggers.Manual],
            StepDefinitions = [Commands.Diagnostics, Commands.InputValue],
            WorkflowTypes = [Github.WorkflowType],
        },
    ];
}