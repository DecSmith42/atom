namespace Atom;

[BuildDefinition]
internal partial class Build : BuildDefinition,
    IAzureKeyVault,
    IAzureArtifactStorage,
    IPackAtom,
    IPackAtomTool,
    IPackAzureKeyVaultExtension,
    IPackAzureStorageExtension,
    IPackDotnetExtension,
    IPackGithubWorkflowsExtension,
    IPackGitVersioningExtension,
    IPushToNuget,
    ITestAtom,
    IPrepareRelease
{
    public override IReadOnlyList<IWorkflowOption> DefaultWorkflowOptions =>
    [
        AzureKeyVault.UseAzureKeyVault, Artifacts.UseCustomArtifactProvider,
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("Validate")
        {
            Triggers = [Github.Triggers.Manual, Github.Triggers.PullIntoMain],
            StepDefinitions =
            [
                Commands.PackAtom,
                Commands.PackAtomTool,
                Commands.PackAzureKeyVaultExtension,
                Commands.PackAzureStorageExtension,
                Commands.PackDotnetExtension,
                Commands.PackGithubWorkflowsExtension,
                Commands.PackGitVersioningExtension,
                Commands.TestAtom,
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
                Commands.PackAzureKeyVaultExtension,
                Commands.PackAzureStorageExtension,
                Commands.PackDotnetExtension,
                Commands.PackGithubWorkflowsExtension,
                Commands.PackGitVersioningExtension,
                Commands.TestAtom,
                Commands.PushToNuget,
            ],
            WorkflowTypes = [Github.WorkflowType],
        },
        new("Prepare Release")
        {
            Triggers =
            [
                new GithubManualTrigger([
                    GithubManualChoiceInput.ForParam(ParamDefinitions[Params.PrereleaseTag], false, ["alpha", "beta", "rc"]),
                ]),
            ],
            StepDefinitions = [Commands.PrepareRelease],
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