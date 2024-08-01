AtomHost.Run<Build>(args);

[BuildDefinition]
internal partial class Build : BuildDefinition,
    IAzureKeyVault,
    IAzureArtifactStorage,
    IGithubWorkflows,
    IGitVersion,
    IPackAtom,
    IPackAtomTool,
    IPackAzureKeyVaultExtension,
    IPackAzureStorageExtension,
    IPackDotnetExtension,
    IPackGithubWorkflowsExtension,
    IPackGitVersionExtension,
    IPackAtomSourceGenerators,
    IPushToNuget,
    ITestAtom
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
                Commands.PackAtom.WithSuppressedArtifactPublishing,
                Commands.PackAtomTool.WithSuppressedArtifactPublishing,
                Commands.PackAzureKeyVaultExtension.WithSuppressedArtifactPublishing,
                Commands.PackAzureStorageExtension.WithSuppressedArtifactPublishing,
                Commands.PackDotnetExtension.WithSuppressedArtifactPublishing,
                Commands.PackGithubWorkflowsExtension.WithSuppressedArtifactPublishing,
                Commands.PackGitVersionExtension.WithSuppressedArtifactPublishing,
                Commands.PackAtomSourceGenerators.WithSuppressedArtifactPublishing,
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
                Commands.PackGitVersionExtension,
                Commands.PackAtomSourceGenerators,
                Commands.TestAtom,
                Commands.PushToNuget,
            ],
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