namespace Atom;

[BuildDefinition]
internal partial class Build : BuildDefinition,
    IAzureKeyVault,
    IAzureArtifactStorage,
    IPackAtom,
    IPackAtomGithubWorkflows,
    IPackAtomSourceGenerators,
    IPushToNuget,
    IDiagnostics,
    IPackAtomTool,
    IInputValue,
    IGetVaultSecret
{
    public override IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions =>
    [
        AzureKeyVault.UseAzureKeyVault, Artifacts.UseArtifactProvider,
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("Validate")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PullIntoMain],
            StepDefinitions =
            [
                Commands.PackAtom, Commands.PackAtomTool, Commands.PackAtomGithubWorkflows, Commands.PackAtomSourceGenerators,
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        new("Build")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PushToMain],
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
            StepDefinitions = [Commands.Diagnostics, Commands.InputValue, Commands.GetVaultSecret],
            WorkflowTypes = [Github.WorkflowType],
        },
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
    ];
}